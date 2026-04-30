using System;
using System.Collections.Generic;
using System.Reflection;
using DrakeToolbox.Services;
using DrakeToolbox.Cast;
using DrakeToolbox.Reflection;

namespace DrakeToolbox.Blueprints
{
    public sealed class BlueprintBinder : IService
    {
        public bool IsPersistent => true;

        private BlueprintRegistry BlueprintRegistry => ServiceProvider.Instance.GetService<BlueprintRegistry>();

        private Dictionary<Type, FieldInfo[]> fieldsInType;
        private Dictionary<FieldInfo, (bool hasAttribute, BlueprintParameterAttribute attribute)> attributeInFields;

        public BlueprintBinder()
        {
            fieldsInType = new Dictionary<Type, FieldInfo[]>();
            attributeInFields = new Dictionary<FieldInfo, (bool hasAttribute, BlueprintParameterAttribute attribute)>();
        }

        public void Apply(ref object instance, string blueprintTable, string blueprintId)
        {
            Type instanceType = instance.GetType();
            do
            {
                foreach (FieldInfo field in GetFields(instanceType))
                {
                    if (!field.FieldType.IsPrimitive && field.FieldType != typeof(string))
                    {
                        object classFieldValue = field.GetValue(instance);
                        Apply(ref classFieldValue, blueprintTable, blueprintId);
                        continue;
                    }

                    (bool hasAttribute, BlueprintParameterAttribute attribute) blueprintParameter =
                        GetBlueprintParameterAttribute(field);

                    if (blueprintParameter.hasAttribute)
                    {
                        field.SetValue(instance, StringCast.Convert(
                            BlueprintRegistry.BlueprintDatas[blueprintTable]
                                [blueprintId, blueprintParameter.attribute.ParameterHeader],
                            field.FieldType));
                    }
                }

                instanceType = instanceType.BaseType;
            } while (instanceType != typeof(object));
        }

        private FieldInfo[] GetFields(Type type)
        {
            if (!fieldsInType.ContainsKey(type))
                fieldsInType.Add(type, type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            return fieldsInType[type];
        }

        private (bool hasAttribute, BlueprintParameterAttribute attribute) GetBlueprintParameterAttribute(FieldInfo field)
        {
            bool contains = false;
            if (!attributeInFields.ContainsKey(field))
            {
                foreach (Attribute attribute in field.GetCustomAttributes())
                {
                    if (attribute is BlueprintParameterAttribute)
                    {
                        attributeInFields.Add(field, (true, attribute as BlueprintParameterAttribute));
                        contains = true;
                        break;
                    }
                }
            }

            if (!contains)
            {
                attributeInFields.Add(field, (false, null));
            }

            return attributeInFields[field];
        }
    }
}