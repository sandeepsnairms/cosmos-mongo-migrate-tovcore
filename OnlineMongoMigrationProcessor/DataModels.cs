﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace OnlineMongoMigrationProcessor
{
    public class Joblist
    {
        public List<MigrationJob> MigrationJobs;

        public int activeRestoreProcessId=0;
        public int activeDumpProcessId=0;

        private string filePath;

        public Joblist()
        {
            if (!System.IO.Directory.Exists($"{Path.GetTempPath()}migrationjobs"))
            {
                System.IO.Directory.CreateDirectory($"{Path.GetTempPath()}migrationjobs");
            }
            this.filePath = $"{Path.GetTempPath()}migrationjobs\\list.json";

        }

        public void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var loadedObject = JsonConvert.DeserializeObject<Joblist>(json);
                    if (loadedObject != null)
                    {
                        this.MigrationJobs = loadedObject.MigrationJobs;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error loading data: {ex.Message}");
            }
        }


        public bool Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(filePath, json);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error saving data: {ex.Message}",LogType.Error);
                return false;
            }
        }
    }

    public class MigrationJob
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SourceEndpoint { get; set; }
        public string TargetEndpoint { get; set; }
        [JsonIgnore]
        public string SourceConnectionString { get; set; }
        [JsonIgnore]
        public string TargetConnectionString { get; set; }
        public string NameSpaces { get; set; }
        public DateTime? StartedOn { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsOnline { get; set; }
        public bool IsCancelled { get; set; }
        public bool IsStarted { get; set; }
        public bool CurrentlyActive { get; set; }
        public List<MigrationUnit> MigrationUnits { get; set; }
    }

    public class MigrationUnit
    {
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
        public string resumeToken { get; set; }
        public DateTime cursorUtcTimestamp { get; set; }
        public Double DumpPercent { get; set; }
        public Double RestorePercent { get; set; }
        public bool DumpComplete { get; set; }
        public bool RestoreComplete { get; set; }
        public long EstimatedDocCount { get; set; }
        public long ActualDocCount { get; set; }
        public long DumpGap { get; set; }
        public long RestoreGap { get; set; }
        public List<MigrationChunk> MigrationChunks { get; set; }

        public MigrationUnit(string DatabaseName, string CollectionName, List<MigrationChunk> MigrationChunks) 
        {
            this.DatabaseName = DatabaseName;
            this.CollectionName = CollectionName;   
            this.MigrationChunks = MigrationChunks;
        }
    }

    public class LogObject
    {
        public LogObject( LogType type, string message)
        {
            Message = message;
            Type = type;
            Datetime = System.DateTime.Now;
        }

        public string Message { get; set; }
        public LogType Type { get; set; }
        public DateTime Datetime { get; set; }
    }

    public enum LogType { Error, Messge};

    public class MigrationChunk
    {
        public string Lt { get; set; }
        public string Gte { get; set; }
        public bool IsDownloaded { get; set; }
        public bool IsUploaded { get; set; }
        public long DumpQueryDocCount { get; set; }
        public long DumpResultDocCount { get; set; }
        public long RestoredSucessDocCount { get; set; }
        public long RestoredFailedDocCount { get; set; }

        public MigrationChunk(string strtId, string endId, bool downloaded, bool uploaded)
        {
            this.Lt = endId;
            this.Gte = strtId;
            this.IsDownloaded = downloaded;
            this.IsUploaded = uploaded;
        }
    }
}

