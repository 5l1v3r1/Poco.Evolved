﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

using Poco.Evolved.Core.Model;

using Poco.Evolved.SQL.Transactions;

namespace Poco.Evolved.SQL.Database
{
    /// <summary>
    /// Helper for database specific operations on firebird databases.
    /// </summary>
    public class FirebirdDatabaseHelper : SQLDatabaseHelper
    {
        /// <summary>
        /// Constructs a new <see cref="FirebirdDatabaseHelper" />.
        /// </summary>
        /// <param name="installedVersionsTableName">Optional name of the table for saving the information about installed versions</param>
        /// <param name="skipInitInstalledVersions">
        /// Optionally skip the CREATE TABLE statement for the table storing the information about installed versions (the table must be created manually beforehand)
        /// </param>
        public FirebirdDatabaseHelper(string installedVersionsTableName = null, bool skipInitInstalledVersions = false)
            : base(installedVersionsTableName, skipInitInstalledVersions) { }

        /// <summary>
        /// Gets the SQL script to create the table for saving information on installed versions.
        /// </summary>
        /// <returns></returns>
        protected override string GetCreateInstalledVersionsTableScript()
        {
            return string.Format(
                "CREATE TABLE {0} ({1}, {2}, {3}, {4}, {5})",
                m_installedVersionsTableName,
                ConvertToNameOnDatabase(nameof(InstalledVersion.VersionNumber)) + " BIGINT NOT NULL PRIMARY KEY",
                ConvertToNameOnDatabase(nameof(InstalledVersion.Description)) + " VARCHAR(1024)",
                ConvertToNameOnDatabase(nameof(InstalledVersion.Installed)) + " VARCHAR(38) NOT NULL",
                ConvertToNameOnDatabase(nameof(InstalledVersion.ExecutionTime)) + " BIGINT NOT NULL",
                ConvertToNameOnDatabase(nameof(InstalledVersion.Checksum)) + " VARCHAR(1024)"
            );
        }

        /// <summary>
        /// Creates the table for saving information on installed versions if necessary.
        /// </summary>
        /// <param name="unitOfWork">The unit of work to work with</param>
        public override void InitInstalledVersions(SQLUnitOfWork unitOfWork)
        {
            if (!m_skipInitInstalledVersions)
            {
                bool tableExists = false;

                using (IDbCommand command = unitOfWork.Connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM RDB$RELATIONS WHERE RDB$RELATION_NAME = '" + m_installedVersionsTableName + "'";
                    command.Transaction = unitOfWork.Transaction;

                    tableExists = ((long)command.ExecuteScalar()) > 0;
                }

                if (!tableExists)
                {
                    using (IDbCommand command = unitOfWork.Connection.CreateCommand())
                    {
                        command.CommandText = GetCreateInstalledVersionsTableScript();
                        command.Transaction = unitOfWork.Transaction;

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
