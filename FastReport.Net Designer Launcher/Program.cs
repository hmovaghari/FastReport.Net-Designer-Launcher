using FastReport.Net_Designer_Launcher.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FastReport.Net_Designer_Launcher
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            CheckInput(args);
            RunFastReportFile(args);
        }

        private static void CheckInput(string[] args)
        {
            ErrorMode errorMode;
            if (IsInputsValid(args, out errorMode))
            {
                IsResetMode(args);
            }
            else
            {
                CheckError(errorMode, args);
            }
        }

        private static bool IsInputsValid(string[] args, out ErrorMode errorMode)
        {
            bool result = true;
            errorMode = ErrorMode.NoError;

            if (
                (args.Count() == 1 && args[0].ToLower() != "reset")
                ^
                (args.Count() == 0 || args.Count() > 1)
                )
            {
                if (string.IsNullOrEmpty(Settings.Default.FastReportAddress))
                {
                    errorMode = ErrorMode.AppNotSet;
                    result = false;
                }
                else if (!File.Exists(Settings.Default.FastReportAddress + "\\Designer.exe"))
                {
                    Settings.Default.FastReportAddress = string.Empty;
                    Settings.Default.Save();
                    errorMode = ErrorMode.AppNotFind;
                    result = false;
                }
            }

            if (args.Count() > 1)
            {
                errorMode = ErrorMode.ArgumentsNotValid;
                result = false;
            }
            else if (args.Count() == 1 && args[0].ToLower() != "reset" && !File.Exists(args[0]))
            {
                errorMode = ErrorMode.ArgumentsNotFind;
                result = false;
            }

            return result;
        }

        private static void IsResetMode(string[] args)
        {
            if (args.Count() == 1 && args[0].ToLower() == "reset")
            {
                MessageBox.Show(Resources.ChangeInstaledDirectory, Resources.ApplicationName);
                ChangeInstallPatOfFastReport(args);
                Environment.Exit(0);
            }
        }

        private static void CheckError(ErrorMode errorMode, string[] args)
        {
            switch (errorMode)
            {
                case ErrorMode.NoError:
                    break;
                case ErrorMode.AppNotSet:
                    MessageBox.Show(Resources.ChangeInstaledDirectory, Resources.ApplicationName);
                    ChangeInstallPatOfFastReport(args);
                    break;
                case ErrorMode.AppNotFind:
                    MessageBox.Show(Resources.InstalledDirectoryInvalid, Resources.ApplicationName);
                    ChangeInstallPatOfFastReport(args);
                    break;
                case ErrorMode.ArgumentsNotValid:
                    MessageBox.Show(Resources.InvalidArguments, Resources.ApplicationName);
                    Environment.Exit(0);
                    break;
                case ErrorMode.ArgumentsNotFind:
                    MessageBox.Show(Resources.FastReportFileNotFind, Resources.ApplicationName);
                    Environment.Exit(0);
                    break;
                default:
                    break;
            }
        }

        private static void ChangeInstallPatOfFastReport(string[] args)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel)
            {
                Environment.Exit(0);
            }
            Settings.Default.FastReportAddress = folderBrowserDialog.SelectedPath;
            Settings.Default.Save();
            if (!File.Exists(Settings.Default.FastReportAddress + "\\Designer.exe"))
            {
                Settings.Default.FastReportAddress = string.Empty;
                Settings.Default.Save();
                MessageBox.Show(Resources.InstalledDirectoryInvalid, Resources.ApplicationName);
                ChangeInstallPatOfFastReport(args);
            }
        }

        private static void RunFastReportFile(string[] args)
        {
            //////////////////////////////////////////////////////

            Process process = new Process();
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo.FileName = "cmd.exe";
            StringBuilder command = new StringBuilder();
            command.Append("/c ");
            command.Append("del \"%USERPROFILE%\\AppData\\Local\\FastReport\\FastReport.config\"");
            command.Append(" && ");
            command.Append($"\"{Settings.Default.FastReportAddress}\\Designer.exe\"");
            command.Append(args.Count() == 1 ? $" \"{args[0]}\"" : string.Empty);
            command.Append(" && exit");
            process.StartInfo.Arguments = command.ToString();
            process.Start();
        }
    }
}
