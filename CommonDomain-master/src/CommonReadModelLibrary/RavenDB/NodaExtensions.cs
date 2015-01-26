// Copyright 2012 The Noda Time Authors. All rights reserved.
// Use of this source code is governed by the Apache License 2.0,
// as found in the LICENSE.txt file.

using NodaTime;
using Raven.Imports.Newtonsoft.Json;

namespace CommonReadModelLibrary.RavenDB
{
    /// <summary>
    /// Static class containing extension methods to configure Json.NET for Noda Time types.
    /// </summary>
    public static class NodaExtensions
    {
        /// <summary>
        /// Configures json.net with everything required to properly serialize and deserialize NodaTime data types.
        /// </summary>
        public static JsonSerializerSettings ConfigureForNodaTime(this JsonSerializerSettings settings, IDateTimeZoneProvider provider)
        {
            // add our converters
            settings.Converters.Add(NodaConverters.InstantConverter);
            settings.Converters.Add(NodaConverters.IntervalConverter);
            settings.Converters.Add(NodaConverters.LocalDateConverter);
            settings.Converters.Add(NodaConverters.LocalDateTimeConverter);
            settings.Converters.Add(NodaConverters.LocalTimeConverter);
            settings.Converters.Add(NodaConverters.OffsetConverter);
            settings.Converters.Add(new NodaDateTimeZoneConverter(provider));
            settings.Converters.Add(NodaConverters.DurationConverter);
            settings.Converters.Add(NodaConverters.RoundtripPeriodConverter);
            settings.DateParseHandling = DateParseHandling.None;

            // return to allow fluent chaining if desired
            return settings;
        }

        /// <summary>
        /// Configures json.net with everything required to properly serialize and deserialize NodaTime data types.
        /// </summary>
        public static JsonSerializer ConfigureForNodaTime(this JsonSerializer serializer, IDateTimeZoneProvider provider)
        {
            // add our converters
            serializer.Converters.Add(NodaConverters.InstantConverter);
            serializer.Converters.Add(NodaConverters.IntervalConverter);
            serializer.Converters.Add(NodaConverters.LocalDateConverter);
            serializer.Converters.Add(NodaConverters.LocalDateTimeConverter);
            serializer.Converters.Add(NodaConverters.LocalTimeConverter);
            serializer.Converters.Add(NodaConverters.OffsetConverter);
            serializer.Converters.Add(new NodaDateTimeZoneConverter(provider));
            serializer.Converters.Add(NodaConverters.DurationConverter);
            serializer.Converters.Add(NodaConverters.RoundtripPeriodConverter);
            serializer.DateParseHandling = DateParseHandling.None;

            // return to allow fluent chaining if desired
            return serializer;
        }
    }
}
