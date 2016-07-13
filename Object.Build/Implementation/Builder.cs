using Object.Build.Interface;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Object.Build.Implementation
{
    public class Builder<TObject> :
        IBuilder<TObject>
    {
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
            return Factory<TObject>.Build(_propertyValues, Source);
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
            _propertyValues.TryAdd(propertyName.ToLower(), value);

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
