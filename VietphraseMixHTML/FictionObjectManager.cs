using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Newtonsoft.Json;
using ServiceStack.Text;
using HtmlDocument = System.Windows.Forms.HtmlDocument;
using Setting = VietphraseMixHTML.Properties.Settings;

namespace VietphraseMixHTML
{
    public class FictionObjectManager
    {
        private static FictionObjectManager fictionObjectManager = null;

        public List<FictionObject> ProjectList;
        public FictionObjectManager()
        {
            ProjectList = new List<FictionObject>();
        }
        public static FictionObjectManager Instance
        {
            get
            {
                if(fictionObjectManager == null)
                {
                    fictionObjectManager = new FictionObjectManager();
                }
                return fictionObjectManager;
            }
        }
            
        public void CheckUpdate()
        {
            
        }
        
        public void CheckUpdate(FictionObject fictionObject)
        {
            
        }

        public void Download()
        {
            
        }

        public void Download(FictionObject fictionObject)
        {
             
        }

        public List<string> GetLinks(FictionObject fictionObject)
        {
            return null;
        }

        public void Add(FictionObject fictionObject)
        {
            if(NotInList(fictionObject))
            {
                ProjectList.Add(fictionObject);
            }
        }
        
        public void RemoveAt(int index)
        {
            ProjectList.RemoveAt(index);
        }

        public void Remove(FictionObject fictionObject)
        {
            foreach (FictionObject o in ProjectList)
            {
                if (o.Equals(fictionObject))
                {
                    ProjectList.Remove(o);
                    return;
                }
            }
        }
        #region utilize functions

        private bool NotInList(FictionObject o)
        {
            foreach (FictionObject fictionObject in ProjectList)
            {
                if(fictionObject.Equals(o))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        public void Init()
        {
            // load all fiction objects in workspace
            if(!Directory.Exists(Setting.Default.Workspace))
            {
                Directory.CreateDirectory(Setting.Default.Workspace);
            }
            string[] paths  =Directory.GetDirectories(Setting.Default.Workspace);

            foreach (string path in paths)
            {
                LoadProjects(path); 
            }
        }

        private void LoadProjects(string rootPath)
        {
            string[] file2s = Directory.GetFiles(rootPath, "*.fobj3");
            if (file2s != null && file2s.Length > 0)
            {
                bool loaded = false;
                Stream stream = null;
                foreach (string file in file2s)
                {

                    try
                    {
                        StreamReader writer = new StreamReader(file,                                      
                                      Encoding.UTF8,false);
                        var serializer = new Newtonsoft.Json.JsonSerializer();
                        FictionObject fictionObject = (FictionObject)serializer.Deserialize(writer,typeof(FictionObject));
                        
                        //stream = File.Open(file, FileMode.Open);
                        ////BinaryFormatter bf = new BinaryFormatter();

                        ////FictionObject fictionObject = (FictionObject)bf.Deserialize(stream);
                        //var streamReader = new StreamReader(stream);
                        //var reader = new TypeSerializer<FictionObject>();
                        //FictionObject fictionObject = reader.DeserializeFromReader(streamReader);
                        fictionObject.RecalculatePreviousStepCount();
                        Add(fictionObject);
                        writer.Close();
                        loaded = true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if (stream != null) stream.Close();
                        stream = null;
                    }
                    if(loaded) return;
                }
            }
            string[] files = Directory.GetFiles(rootPath, "*.fobj");
            if (files != null && files.Length > 0)
            {
                Stream stream = null;
                foreach (string file in files)
                {
                    
                    try
                    {
                        stream = File.Open(file, FileMode.Open);
                        BinaryFormatter bf = new BinaryFormatter();

                        FictionObject fictionObject = (FictionObject) bf.Deserialize(stream);
                        String location = fictionObject.Location;
                        int lastIndex = location.LastIndexOf("\\");
                        if (lastIndex > 0) fictionObject.Location = location.Substring(lastIndex + 1);
                        else fictionObject.Location = location;
                        fictionObject.RecalculatePreviousStepCount();
                        Add(fictionObject);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        if(stream!=null) stream.Close();
                        stream = null;
                    }
                }
            }
        }

        public void Save()
        {
            foreach (FictionObject fictionObject in ProjectList)
            {
                fictionObject.Save();
            }
        }

        public void Update()
        {
            foreach (FictionObject fictionObject in ProjectList)
            {
                fictionObject.Update();
            }
        }

        internal void Update(System.ComponentModel.BackgroundWorker backgroundWorker)
        {
            int count = 1;
            foreach (FictionObject fictionObject in ProjectList)
            {
                backgroundWorker.ReportProgress(count++,fictionObject.Name);
                fictionObject.Update();
                
            }
        }
    }
}
