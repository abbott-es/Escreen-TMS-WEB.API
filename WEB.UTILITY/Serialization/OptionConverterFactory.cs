using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WEB.UTILITY.Serialization
{
    public class OptionConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
                typeToConvert.GetGenericTypeDefinition() == typeof(Option<>);
        }

        public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type optionType = typeToConvert.GetGenericArguments()[0];

            return Activator.CreateInstance(
                typeof(OptionConverter<>).MakeGenericType(optionType),
                bindingAttr: BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null) as JsonConverter;
        }

        private class OptionConverter<T> : JsonConverter<Option<T>>
        {
            public override Option<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                return reader.TokenType == JsonTokenType.Null ?
                    default :
                    (Option<T>)(T)JsonSerializer.Deserialize(ref reader, typeof(T), options);
            }

            public override void Write(Utf8JsonWriter writer, Option<T> value, JsonSerializerOptions options)
            {
                value.BiIter(
                    v => JsonSerializer.Serialize(writer, v, typeof(T), options),
                    _ => writer.WriteNullValue());
            }
        }
    }
}
