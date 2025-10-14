using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WEB.UTILITY.Serialization
{
    public static class SerializationBuilder
    {
        public static IMvcBuilder UseSerialization(this IMvcBuilder builder)
        {
            builder.AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                opts.JsonSerializerOptions.Converters.Add(new OptionConverterFactory());
            });
            return builder;
        }

        public static JsonSerializerOptions Options()
        {
            var options = new JsonSerializerOptions();
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new OptionConverterFactory());
            return options;
        }
    }
}
