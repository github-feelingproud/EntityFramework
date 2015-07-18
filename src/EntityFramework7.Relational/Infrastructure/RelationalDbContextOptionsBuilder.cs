// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TBuilder : RelationalDbContextOptionsBuilder<TBuilder, TExtension>
        where TExtension : RelationalOptionsExtension
    {
        protected RelationalDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        protected abstract TExtension CloneExtension();

        public virtual TBuilder MaxBatchSize(int maxBatchSize)
            => SetOption(e => e.MaxBatchSize = maxBatchSize);

        public virtual TBuilder CommandTimeout(int? commandTimeout)
            => SetOption(e => e.CommandTimeout = commandTimeout);

        public virtual TBuilder MigrationsAssembly([NotNull] string assemblyName)
            => SetOption(e => e.MigrationsAssembly = Check.NullButNotEmpty(assemblyName, nameof(assemblyName)));

        public virtual TBuilder SuppressAmbientTransactionWarning()
            => SetOption(e => e.ThrowOnAmbientTransaction = false);

        protected virtual TBuilder SetOption([NotNull] Action<TExtension> setAction)
        {
            Check.NotNull(setAction, nameof(setAction));

            var extension = CloneExtension();

            setAction(extension);

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return (TBuilder)this;
        }
    }
}
