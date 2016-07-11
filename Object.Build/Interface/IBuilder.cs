using System;
using System.Linq.Expressions;

namespace Object.Build.Interface
{
    public interface IBuilder<TObject>
    {
        TObject Source { get; }

        IBuilder<TObject> Set<TPropertyType>(
            string propertyName, 
            TPropertyType value);

        IBuilder<TObject> Set<TPropertyType>(
            string propertyName, 
            Func<TObject, TPropertyType> valueProvider);

        IBuilder<TObject> Set<TPropertyType>(
            Expression<Func<TObject, TPropertyType>> propertyExpression, 
            TPropertyType value);

        IBuilder<TObject> Set<TPropertyType>(
            Expression<Func<TObject, TPropertyType>> propertyExpression, 
            Func<TObject, TPropertyType> valueProvider);

        TObject Build();
    }
}
