﻿using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Content.Server.Database
{
    public sealed class SqliteServerDbContext : ServerDbContext
    {
        public SqliteServerDbContext(DbContextOptions<SqliteServerDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            ((IDbContextOptionsBuilderInfrastructure) options).AddOrUpdateExtension(new SnakeCaseExtension());

            options.ConfigureWarnings(x =>
            {
                x.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
#if DEBUG
                // for tests
                x.Ignore(CoreEventId.SensitiveDataLoggingEnabledWarning);
#endif
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
#endif
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var ipConverter = new ValueConverter<IPAddress, string>(
                v => v.ToString(),
                v => IPAddress.Parse(v));

            modelBuilder.Entity<Player>()
                .Property(p => p.LastSeenAddress)
                .HasConversion(ipConverter);

            var ipMaskConverter = new ValueConverter<(IPAddress address, int mask), string>(
                v => InetToString(v.address, v.mask),
                v => StringToInet(v)
            );

            modelBuilder
                .Entity<ServerBan>()
                .Property(e => e.Address)
                .HasColumnType("TEXT")
                .HasConversion(ipMaskConverter);

            var jsonConverter = new ValueConverter<JsonDocument, string>(
                v => JsonDocumentToString(v),
                v => StringToJsonDocument(v));

            modelBuilder.Entity<AdminLog>()
                .Property(log => log.Json)
                .HasConversion(jsonConverter);
        }

        private static string InetToString(IPAddress address, int mask) {
            if (address.IsIPv4MappedToIPv6)
            {
                // Fix IPv6-mapped IPv4 addresses
                // So that IPv4 addresses are consistent between separate-socket and dual-stack socket modes.
                address = address.MapToIPv4();
                mask -= 96;
            }
            return $"{address}/{mask}";
        }

        private static (IPAddress, int) StringToInet(string inet) {
            var idx = inet.IndexOf('/', StringComparison.Ordinal);
            return (
                IPAddress.Parse(inet.AsSpan(0, idx)),
                int.Parse(inet.AsSpan(idx + 1), provider: CultureInfo.InvariantCulture)
            );
        }

        private static string JsonDocumentToString(JsonDocument document)
        {
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = false});

            document.WriteTo(writer);
            writer.Flush();

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static JsonDocument StringToJsonDocument(string str)
        {
            return JsonDocument.Parse(str);
        }
    }
}
