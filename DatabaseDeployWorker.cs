namespace Navicon.SP.Components.SqlCache
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using Microsoft.SharePoint;
    using Microsoft.SharePoint.Administration;
    using Microsoft.SharePoint.Utilities;
    using Microsoft.SqlServer.Dac;

    using Navicon.SP.Common;
    using Navicon.SP.JobDefinitions;

    public class DatabaseDeployWorker : IExecutor
    {
        private const string JobName = "ArchiveDataBaseDeploymentJob";
        public Guid SiteId { get; set; }

        public void Execute(SPJobDefinition jobInstance)
        {
            SPSecurity.RunWithElevatedPrivileges(
                delegate
                    {
                        using (SPSite spSite = new SPSite(this.SiteId))
                        {
                            const string createDataBaseFolderPath = @"Template\SQL\Navicon.SP.DataBase.dacpac";
                            string connectionString = spSite.ContentDatabase.DatabaseConnectionString;
                            string archiveDacpacPath = SPUtility.GetVersionedGenericSetupPath(
                                createDataBaseFolderPath,
                                15);
                            Logger.WriteMessage(
                                "Путь до файла sql скрипта: {0}, connection string: {1}, учетная запись: {2}  ",
                                archiveDacpacPath,
                                connectionString,
                                spSite.RootWeb.CurrentUser.LoginName);

                            DacServices dacServices = new DacServices(connectionString);
                            dacServices.Message += (sender, args) =>
                                {
                                    Trace.WriteLine(args.Message);
                                    Logger.WriteMessage(args.Message.Message);
                                };
                            dacServices.ProgressChanged += (sender, args) =>
                                {
                                    Trace.WriteLine(args.Message);
                                    Logger.WriteMessage(args.Message);
                                };

                            using (DacPackage package = DacPackage.Load(archiveDacpacPath))
                            {
                                DacDeployOptions options = new DacDeployOptions { CreateNewDatabase = false };
                                dacServices.Deploy(
                                    package,
                                    Constants.DataBaseName,
                                    true,
                                    options,
                                    new CancellationToken?());
                            }
                        }
                    });
        }

        public static void Deploy(Guid siteId)
        {
            DatabaseDeployWorker databaseDeployWorker = new DatabaseDeployWorker {SiteId = siteId};
            databaseDeployWorker.Work();
        }

        private void Work()
        {
            try
            {
                NowOnlyOnceJob.RunNowInJobMyCode(this.SiteId, JobName, this, true);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Не удалось развернуть базу данных.";
                Logger.ShowErrorOnPage(errorMessage, ex);
            }
        }
    }
}