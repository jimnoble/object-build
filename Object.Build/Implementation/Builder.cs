using Object.Build.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Object.Build.Implementation
{
    public class Builder<TObject> :
        IBuilder<TObject>
    {
        static readonly Func<ConcurrentDictionary<string, object>, TObject, TObject> _factoryFunc;

        static Builder()
        {
            _factoryFunc = BuildFactoryFunc();
        }

        static Func<ConcurrentDictionary<string, object>, TObject, TObject> BuildFactoryFunc()
        {
            var type = typeof(TObject);

            var constructors = type
                .GetConstructors()
                .Select(ci => new
                {
                    ConstructorInfo = ci,

                    Parameters = ci.GetParameters()
                })
                .ToList();
            
            var constructor = constructors
                .OrderByDescending(c => c.Parameters.Count())
                .First();

            var arguments = new List<Expression>();

            var dictionaryParameter = Expression.Parameter(
                typeof(ConcurrentDictionary<string, object>), 
                "d");

            var sourceParameter = Expression.Parameter(
                typeof(TObject),
                "s");

            var concurrentDictionaryType = typeof(ConcurrentDictionary<string, object>);

            var getOrAddMethodInfo = concurrentDictionaryType.GetMethod(
                "GetOrAdd",
                new Type[] { typeof(string), typeof(object) });

            var properties = type
                .GetProperties()
                .ToDictionary(p => p.Name.ToLower(), p => p);

            var propertiesToSet = properties
                .Where(p => p.Value.SetMethod != null)
                .ToDictionary(p => p.Key, p => p.Value);

            foreach (var parameter in constructor.Parameters)
            {
                var key = parameter.Name.ToLower();

                var propertyType = parameter.ParameterType;

                arguments.Add(BuildArgumentExpression(
                    dictionaryParameter, 
                    sourceParameter,
                    getOrAddMethodInfo, 
                    key, 
                    properties[key]));

                propertiesToSet.Remove(key);
            }

            var newExpression = Expression.New(
                constructor.ConstructorInfo, 
                arguments);

            var outerExpression = newExpression as Expression;

            if(propertiesToSet.Any())
            {
                var memberBindings = propertiesToSet
                    .Select(p => Expression.Bind(
                        p.Value, 
                        BuildArgumentExpression(
                            dictionaryParameter, 
                            sourceParameter,
                            getOrAddMethodInfo, 
                            p.Key, 
                            p.Value)))
                    .ToList();

                outerExpression = Expression.MemberInit(newExpression, memberBindings);
            }

            var lambda = Expression.Lambda<Func<ConcurrentDictionary<string, object>, TObject, TObject>>(
                outerExpression,
                dictionaryParameter,
                sourceParameter);

            return lambda.Compile();
        }

        static UnaryExpression BuildArgumentExpression(
            ParameterExpression dictionaryParameter,
            ParameterExpression sourceParameter,
            MethodInfo getOrAddMethodInfo, 
            string key, 
            PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            return Expression.Convert(
                Expression.Call(
                    dictionaryParameter,
                    getOrAddMethodInfo,
                    Expression.Constant(key),
                    Expression.TypeAs(
                        Expression.Condition(
                            Expression.Equal(
                                sourceParameter, 
                                Expression.Constant(default(TObject))),
                            Expression.Default(propertyType),
                            Expression.Property(
                                sourceParameter, 
                                propertyInfo.GetMethod)),
                        typeof(object))),
                propertyType);
        }

        readonly ConcurrentDictionary<string, object> _propertyValues = 
            new ConcurrentDictionary<string, object>();

        public Builder()
        {
        }

        public Builder(TObject source)
        {
            Source = source;
        }

        public TObject Source { get; }

        public TObject Build()
        {
            return _factoryFunc(_propertyValues, Source);
        }

        public IBuilder<TObject> Set<TPropertyType>(
            Expression<Func<TObject, TPropertyType>> propertyExpression, 
            Func<TObject, TPropertyType> valueProvider)
        {
            throw new NotImplementedException();
        }

        public IBuilder<TObject> Set<TPropertyType>(
            Expression<Func<TObject, TPropertyType>> propertyExpression, 
            TPropertyType value)
        {
            var memberInfo = GetMemberInfo(propertyExpression);

            _propertyValues.TryAdd(memberInfo.Name.ToLower(), value);

            return this;
        }

        public IBuilder<TObject> Set<TPropertyType>(
            string propertyName, 
            Func<TObject, TPropertyType> valueProvider)
        {
            throw new NotImplementedException();
        }

        public IBuilder<TObject> Set<TTPropertyType>(
            string propertyName, 
            TTPropertyType value)
        {
            _propertyValues.TryAdd(propertyName, value);

            return this;
        }

        static MemberInfo GetMemberInfo<TParam, TResult>(
            Expression<Func<TParam, TResult>> expression)
        {
            var member = expression.Body as MemberExpression;

            if (member != null)
            {
                return member.Member;
            }                

            throw new ArgumentException(
                "Expression is not a member access", 
                "expression");
        }
    }
}
