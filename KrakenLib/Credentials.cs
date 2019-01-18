using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace theCodeJerk.apis.Kraken
{
    public sealed class Credentials
    {
        public string BaseAddress { get; set; }
        public string Version { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }

        private static readonly Credentials instance = new Credentials();
        static Credentials()
        {

        }
        private Credentials()
        {
            Load();
        }
        public static Credentials Instance
        {
            get { return instance; }
        }
        public void Load()
        {
            IniParser parser = new IniParser(IniPath());
            BaseAddress = parser.GetSetting("Credentials", "BaseAddress");
            Version = parser.GetSetting("Credentials", "Version");
            Key = parser.GetSetting("Credentials", "Key");
            Secret = parser.GetSetting("Credentials", "Secret");
        }
        public void Save()
        {
            IniParser parser = new IniParser(IniPath());
            parser.AddSetting("Credentials", "BaseAddress", BaseAddress);
            parser.AddSetting("Credentials", "Version", Version);
            parser.AddSetting("Credentials", "Key", Key);
            parser.AddSetting("Credentials", "Secret", Secret);
            parser.SaveSettings(IniPath());
        }
        public Boolean Valid()
        {
            Boolean retval = true;
            if (retval) { retval = BaseAddress != ""; }
            if (retval) { retval = Version != ""; }
            if (retval) { retval = Key != ""; }
            if (retval) { retval = Secret != ""; }
            if (retval) { retval = BaseAddress != null; }
            if (retval) { retval = Version != null; }
            if (retval) { retval = Key != null; }
            if (retval) { retval = Secret != null; }
            return retval;
        }
        private string IniPath()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            path = System.IO.Path.Combine(path, "krakenapi.ini");
            return path;
        }
        /// <summary>
        /// Used internally to persist the credentials.
        /// From: https://bytes.com/topic/net/insights/797169-reading-parsing-ini-file-c
        /// </summary>
        private class IniParser
        {
            private Hashtable keyPairs = new Hashtable();
            private String iniFilePath;

            private struct SectionPair
            {
                public String Section;
                public String Key;
            }

            /// <summary>
            /// Opens the INI file at the given path and enumerates the values in the IniParser.
            /// </summary>
            /// <param name="iniPath">Full path to INI file.</param>
            public IniParser(String iniPath)
            {
                TextReader iniFile = null;
                String strLine = null;
                String currentRoot = null;
                String[] keyPair = null;

                iniFilePath = iniPath;

                if (File.Exists(iniPath))
                {
                    try
                    {
                        iniFile = new StreamReader(iniPath);

                        strLine = iniFile.ReadLine();

                        while (strLine != null)
                        {
                            strLine = strLine.Trim();//.ToUpper();

                            if (strLine != "")
                            {
                                if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                                {
                                    currentRoot = strLine.Substring(1, strLine.Length - 2);
                                }
                                else
                                {
                                    keyPair = strLine.Split(new char[] { '=' }, 2);

                                    SectionPair sectionPair;
                                    String value = null;

                                    if (currentRoot == null)
                                        currentRoot = "ROOT";

                                    sectionPair.Section = currentRoot;
                                    sectionPair.Key = keyPair[0];

                                    if (keyPair.Length > 1)
                                        value = keyPair[1];

                                    keyPairs.Add(sectionPair, value);
                                }
                            }

                            strLine = iniFile.ReadLine();
                        }

                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (iniFile != null)
                            iniFile.Close();
                    }
                }
               // else
                   // throw new FileNotFoundException("Unable to locate " + iniPath);

            }

            /// <summary>
            /// Returns the value for the given section, key pair.
            /// </summary>
            /// <param name="sectionName">Section name.</param>
            /// <param name="settingName">Key name.</param>
            public String GetSetting(String sectionName, String settingName)
            {
                SectionPair sectionPair;
                sectionPair.Section = sectionName.ToUpper();
                sectionPair.Key = settingName.ToUpper();

                return (String)keyPairs[sectionPair];
            }

            /// <summary>
            /// Enumerates all lines for given section.
            /// </summary>
            /// <param name="sectionName">Section to enum.</param>
            public String[] EnumSection(String sectionName)
            {
                ArrayList tmpArray = new ArrayList();

                foreach (SectionPair pair in keyPairs.Keys)
                {
                    if (pair.Section == sectionName.ToUpper())
                        tmpArray.Add(pair.Key);
                }

                return (String[])tmpArray.ToArray(typeof(String));
            }

            /// <summary>
            /// Adds or replaces a setting to the table to be saved.
            /// </summary>
            /// <param name="sectionName">Section to add under.</param>
            /// <param name="settingName">Key name to add.</param>
            /// <param name="settingValue">Value of key.</param>
            public void AddSetting(String sectionName, String settingName, String settingValue)
            {
                SectionPair sectionPair;
                sectionPair.Section = sectionName.ToUpper();
                sectionPair.Key = settingName.ToUpper();

                if (keyPairs.ContainsKey(sectionPair))
                    keyPairs.Remove(sectionPair);

                keyPairs.Add(sectionPair, settingValue);
            }

            /// <summary>
            /// Adds or replaces a setting to the table to be saved with a null value.
            /// </summary>
            /// <param name="sectionName">Section to add under.</param>
            /// <param name="settingName">Key name to add.</param>
            public void AddSetting(String sectionName, String settingName)
            {
                AddSetting(sectionName, settingName, null);
            }

            /// <summary>
            /// Remove a setting.
            /// </summary>
            /// <param name="sectionName">Section to add under.</param>
            /// <param name="settingName">Key name to add.</param>
            public void DeleteSetting(String sectionName, String settingName)
            {
                SectionPair sectionPair;
                sectionPair.Section = sectionName.ToUpper();
                sectionPair.Key = settingName.ToUpper();

                if (keyPairs.ContainsKey(sectionPair))
                    keyPairs.Remove(sectionPair);
            }

            /// <summary>
            /// Save settings to new file.
            /// </summary>
            /// <param name="newFilePath">New file path.</param>
            public void SaveSettings(String newFilePath)
            {
                ArrayList sections = new ArrayList();
                String tmpValue = "";
                String strToSave = "";

                foreach (SectionPair sectionPair in keyPairs.Keys)
                {
                    if (!sections.Contains(sectionPair.Section))
                        sections.Add(sectionPair.Section);
                }

                foreach (String section in sections)
                {
                    strToSave += ("[" + section + "]\r\n");

                    foreach (SectionPair sectionPair in keyPairs.Keys)
                    {
                        if (sectionPair.Section == section)
                        {
                            tmpValue = (String)keyPairs[sectionPair];

                            if (tmpValue != null)
                                tmpValue = "=" + tmpValue;

                            strToSave += (sectionPair.Key + tmpValue + "\r\n");
                        }
                    }

                    strToSave += "\r\n";
                }

                try
                {
                    TextWriter tw = new StreamWriter(newFilePath);
                    tw.Write(strToSave);
                    tw.Close();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            /// <summary>
            /// Save settings back to ini file.
            /// </summary>
            public void SaveSettings()
            {
                SaveSettings(iniFilePath);
            }
        }

    }
}
