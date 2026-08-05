using System;
using System.Reflection;

public class PropertyOrFieldInfo
{
    MemberInfo reference;
    FieldInfo field => reference as FieldInfo;
    PropertyInfo property => reference as PropertyInfo;

    public Type Type { get 
        {
                if (field != null) 
                    return field.FieldType;
                if (property != null) 
                    return property.PropertyType;
                return null;
            } }
    public string Name { get 
        {
                if (field != null) 
                    return field.Name;
                if (property != null) 
                    return property.Name;
                return null;
            } }
    public PropertyOrFieldInfo(MemberInfo info)
    {
        reference = info;
    }
    public object GetValue(object obj)
    {
        if (field != null)
        {
            return field.GetValue(obj);
        }
        if (property != null)
        {
            return property.GetValue(obj);
        }
        return null;
    }
    public void SetValue(object obj, object value)
    {
        if (field != null) field.SetValue(obj, value);
        else if (property != null) property.SetValue(obj, value);
    }

}