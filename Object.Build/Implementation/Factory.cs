using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Object.Build.Implementation
{
    public static class Factory<TObject>
    {
        static Factory()
        {
            Build = BuildFactoryFunc();
        }

        public static Func<ConcurrentDictionary<string, object>, TObject, TObject> Build { get; }

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

            if (propertiesToSet.Any())
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
    }
}
