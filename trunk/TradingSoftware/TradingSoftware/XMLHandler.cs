using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace TradingSoftware
{
    static class XMLHandler
    {
        private static string settingsFilePath = "settings.xml";
        private static string schemaFilePath = "settings.xsd";

        public static bool CreateSettingsFileIfNecessary()
        {
            if(!File.Exists(settingsFilePath)){
                try
                {
                    XDocument document = new XDocument();
                    XElement rootElement = new XElement("TradingSoftware");
                    rootElement.Add(new XAttribute("orderId", 1));
                    document.Add(rootElement);

                    document.Save(settingsFilePath);

                    if (ValidateXMLDocument(document))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public static bool checkIfXMLHasWorkers()
        {
            lock (IBID.XMLReadLock)
            {
                if (XDocument.Load(settingsFilePath).Root.Elements("Worker").Any())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool RemoveWorker(string workerSymbol)
        {
            XDocument document = null;
            lock (IBID.XMLReadLock)
            {
                document = XDocument.Load(settingsFilePath);
            }
            foreach(XElement workerElement in document.Root.Elements("Worker"))
            {
                if(workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                {
                    workerElement.Remove();

                    document.Save(settingsFilePath);

                    if (ValidateXMLDocument(document))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return false;
        }

        public static void LoadWorkersFromXML(MainWindow mainWindow)
        {
            if (checkIfXMLHasWorkers())
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }
                List<XElement> workerElements = document.Root.Elements("Worker").ToList();

                foreach (XElement workerElement in workerElements)
                {
                    WorkerTab workerTab = new WorkerTab(mainWindow);

                    bool hasAlgorithmParameters = workerElement.Attribute("hasAlgorithmParameters").Value.Equals("true") ? true : false;

                    string algorithmParameters = "";
                    if (hasAlgorithmParameters && workerElement.Value != null && workerElement.Value.Length != 0)
                    {
                        algorithmParameters = workerElement.Value.Substring(1, workerElement.Value.Length - 1);
                    }

                    Worker worker = new Worker(mainWindow.mainViewModel, workerTab.workerViewModel,
                                               workerElement.Attribute("symbol").Value,
                                               workerElement.Attribute("exchange").Value,
                                               workerElement.Attribute("isTrading").Value.Equals("true") ? true : false,
                                               workerElement.Attribute("barsize").Value,
                                               workerElement.Attribute("dataType").Value,
                                               decimal.Parse(workerElement.Attribute("pricePremiumPercentage").Value, CultureInfo.InvariantCulture),
                                               int.Parse(workerElement.Attribute("roundLotSize").Value, CultureInfo.InvariantCulture),
                                               workerElement.Attribute("isFutureTrading").Value.Equals("true") ? true : false,
                                               int.Parse(workerElement.Attribute("currentPosition").Value, CultureInfo.InvariantCulture),
                                               workerElement.Attribute("shallIgnoreFirstSignal").Value.Equals("true") ? true : false,
                                               hasAlgorithmParameters,
                                               workerElement.Attribute("algorithmFilePath").Value,
                                               algorithmParameters);
                    
                    mainWindow.mainViewModel.Workers.Add(worker);
                    workerTab.setUpTabWorkerConnection(worker);
                    mainWindow.mainViewModel.WorkerViewModels.Add(workerTab.workerViewModel);

                    mainWindow.MainTabControl.Items.Insert(mainWindow.MainTabControl.Items.Count - 1, workerTab);
                    mainWindow.AddSignalBoxToSummary(workerTab.workerViewModel);
                }
                mainWindow.workersGrid.Items.Refresh();
            }
        }

        public static bool CreateWorker(string equity, string exchange, bool isTrading, string barsize, string dataType,
                                        string algorithmFilePath, decimal pricePremiumPercentage, bool isFutureTrading,
                                        int currentPosition, bool shallIgnoreFirstSignal, bool hasAlgorithmParameters,
                                        int roundLotSize, string algorithmParamters)
        {
            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }

                XElement workerElement = new XElement("Worker");
                workerElement.Add(new XAttribute("symbol", equity));
                workerElement.Add(new XAttribute("exchange", exchange));
                workerElement.Add(new XAttribute("isTrading", isTrading));
                workerElement.Add(new XAttribute("barsize", barsize));
                workerElement.Add(new XAttribute("dataType", dataType));
                workerElement.Add(new XAttribute("pricePremiumPercentage", pricePremiumPercentage));
                workerElement.Add(new XAttribute("isFutureTrading", isFutureTrading));
                workerElement.Add(new XAttribute("currentPosition", currentPosition));
                workerElement.Add(new XAttribute("shallIgnoreFirstSignal", shallIgnoreFirstSignal));
                workerElement.Add(new XAttribute("hasAlgorithmParameters", hasAlgorithmParameters));
                workerElement.Add(new XAttribute("algorithmFilePath", algorithmFilePath));

                if (!isFutureTrading)
                {
                    workerElement.Add(new XAttribute("roundLotSize", roundLotSize));
                }
                else
                {
                    workerElement.Add(new XAttribute("roundLotSize", 1));
                }

                if (hasAlgorithmParameters)
                {
                    workerElement.Value = "\n" + algorithmParamters;
                }

                document.Root.Add(workerElement);

                document.Save(settingsFilePath);

                if (ValidateXMLDocument(document))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool ValidateXMLDocument(XDocument documentToValidate)
        {
            bool wasValidationSuccessful = true;
            XDocument doc = null;
            lock (IBID.XMLReadLock)
            {
                doc = XDocument.Load(settingsFilePath);
            }

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, schemaFilePath);

            XmlReaderSettings xrs = new XmlReaderSettings();
            xrs.ValidationType = ValidationType.Schema;
            xrs.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            xrs.Schemas = schemaSet;
            xrs.ValidationEventHandler += (o, s) =>
            {
                wasValidationSuccessful = false;
                //To write validation errors into the console
                //Console.WriteLine("{0}: {1}", s.Severity, s.Message);
            };

            using (XmlReader xr = XmlReader.Create(doc.CreateReader(), xrs))
            {
                while (xr.Read()) { }
            }

            return wasValidationSuccessful;
        }

        public static string ReadValueFromXML(string workerSymbol, string attributeToRead)
        {
            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }

                if (attributeToRead.Equals("orderId"))
                {
                    return document.Root.Attribute("orderId").Value;
                }
                else
                {
                    foreach (XElement workerElement in document.Root.Elements("Worker"))
                    {
                        if (workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                        {
                            if (!attributeToRead.Equals("algorithmParameters"))
                            {
                                return workerElement.Attribute(attributeToRead).Value;
                            }
                            else
                            {
                                return workerElement.Value.Substring(1, workerElement.Value.Length - 1);
                            }
                        }
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool WriteValueToXML(string workerSymbol, string attributeToRead, string valueToWrite)
        {
            bool wasSuccessful = false;

            try
            {
                XDocument document = null;
                lock (IBID.XMLReadLock)
                {
                    document = XDocument.Load(settingsFilePath);
                }

                if (attributeToRead.Equals("orderId"))
                {
                    document.Root.Attribute("orderId").Value = valueToWrite;
                }
                else
                {
                    foreach (XElement workerElement in document.Root.Elements("Worker"))
                    {
                        if (workerElement.Attribute("symbol").Value.Equals(workerSymbol))
                        {
                            //If algorithmParameters shall be written, write them as the Value of the Worker
                            if (!((attributeToRead.Equals("algorithmParameters") ? workerElement.Value : workerElement.Attribute(attributeToRead).Value).Equals(valueToWrite)))
                            {
                                if (attributeToRead.Equals("algorithmParameters"))
                                {
                                    workerElement.Value = "\n" + valueToWrite;
                                }
                                else
                                {
                                    workerElement.Attribute(attributeToRead).Value = valueToWrite;
                                }
                            }
                            else
                            {
                                wasSuccessful = true;
                            }
                        }
                    }
                }

                //Do not save the file again if nothing changed
                if (!wasSuccessful)
                {
                    lock (IBID.XMLReadLock)
                    {
                        document.Save(settingsFilePath);

                        if (ValidateXMLDocument(document))
                        {
                            wasSuccessful = true;
                        }
                        else
                        {
                            wasSuccessful = false;
                        }
                    }
                }

                return wasSuccessful;
            }
            catch
            {
                return wasSuccessful;
            }
        }
    }
}
