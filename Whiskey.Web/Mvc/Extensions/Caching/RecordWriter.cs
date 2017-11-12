
//  <copyright file="RecordWriter.cs" company="优维拉软件设计工作室">
//      Copyright (c) 2015 www.ovisa.cn All rights reserved.


//  <last-date>2015-01-18 13:16</last-date>


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


namespace Whiskey.Web.Mvc.Extensions
{
    internal class RecordWriter : TextWriter
    {
        private readonly TextWriter _innerWriter;
        private readonly List<StringBuilder> _recorders = new List<StringBuilder>();

        public RecordWriter(TextWriter innerWriter)
        {
            _innerWriter = innerWriter;
        }

        public override Encoding Encoding { get { return _innerWriter.Encoding; } }

        public override void Write(char value)
        {
            _innerWriter.Write(value);

            if (_recorders.Count > 0)
            {
                foreach (StringBuilder recorder in _recorders)
                {
                    recorder.Append(value);
                }
            }
        }

        public override void Write(string value)
        {
            if (value != null)
            {
                _innerWriter.Write(value);

                if (_recorders.Count > 0)
                {
                    foreach (StringBuilder recorder in _recorders)
                    {
                        recorder.Append(value);
                    }
                }
            }
        }

        public override void Write(char[] buffer, int index, int count)
        {
            _innerWriter.Write(buffer, index, count);

            if (_recorders.Count > 0)
            {
                foreach (StringBuilder recorder in _recorders)
                {
                    recorder.Append(buffer, index, count);
                }
            }
        }

        public void AddRecorder(StringBuilder recorder)
        {
            _recorders.Add(recorder);
        }

        public void RemoveRecorder(StringBuilder recorder)
        {
            _recorders.Remove(recorder);
        }
    }
}