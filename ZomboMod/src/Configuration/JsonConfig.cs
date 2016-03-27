/*
*    ______               _             __  __           _ 
*   |___  /              | |           |  \/  |         | |
*      / / ___  _ __ ___ | |__   ___   | \  / | ___   __| |
*     / / / _ \| '_ ` _ \| '_ \ / _ \  | |\/| |/ _ \ / _` |
*    / /_| (_) | | | | | | |_) | (_) | | |  | | (_) | (_| |
*   /_____\___/|_| |_| |_|_.__/ \___/  |_|  |_|\___/ \__,_|
*
*          This file is part of ZomboMod Project.
*             https://www.github.com/ZomboMod
*
*             Copyright (C) 2016 Leonardosnt
*          ZomboMod is licensed under CC BY-NC-SA.
*   
*/

using System;
using System.IO;
using Newtonsoft.Json;

namespace ZomboMod.Configuration
{
    public abstract class JsonConfig : IConfig
    {
        [JsonIgnore]
        public string FileName { get; set; } = "config.json";

        public virtual void Load( string filePath )
        {
            if ( File.Exists( filePath ) )
            {
                try
                {
                    JsonConvert.PopulateObject( File.ReadAllText( filePath ), this );
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine( $"Invalid configuration '{FileName}'.");
                    Console.WriteLine( ex );
                }
            }
            else
            {
                LoadDefaults();
                Save( filePath );
            }
        }

        public virtual void Save( string filePath )
        {
            File.WriteAllText( filePath, "" );

            FileStream fs = null;
            try
            {
                fs = new FileStream( filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite );

                using (TextWriter writer = new StreamWriter( fs ))
                {
                    var jsonWriter = new JsonTextWriter( writer );
                    var serializer = JsonSerializer.Create();

                    serializer.Formatting = Formatting.Indented;
                    serializer.NullValueHandling = NullValueHandling.Ignore;
                    serializer.Serialize( jsonWriter, this );

                    jsonWriter.Close();
                    writer.Close();
                }
            }
            finally
            {
                fs?.Dispose();
            }
        }

        public virtual void LoadDefaults() {}
    }
}
