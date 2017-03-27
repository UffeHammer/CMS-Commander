using System;
using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SitecoreConverter.Core
{
    public class XmlCleanupSoapExtension : SoapExtension
    {
        private Regex replaceRegEx; 

        private Stream oldStream; 
        private Stream newStream; 

        // to modify the content we redirect the stream to a memory stream to allow  
        // easy consumption and modifcation 
        public override Stream ChainStream(Stream stream) 
        { 
            // keep track of the original stream 
            oldStream = stream; 

            // create a new memory stream and configure it as the stream object to use as input and output of the webservice 
            newStream = new MemoryStream(); 
            return newStream; 
        } 

        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute) 
        { 
            // the module is intended to look at all methods. Not on methods tagged with a specific attribute 
            throw new Exception("The method or operation is not implemented."); 
        } 

        public override object GetInitializer(Type serviceType) 
        { 
            // create a compiled instance of the Regular Expression for the chars we would like to replace 
            // add all char points beween 0 and 31 excluding the allowed white spaces (9=TAB, 10=LF, 13=CR) 
            StringBuilder RegExp = new StringBuilder("&#(0"); 
            for (int i = 1; i <= 31; i++) 
            { 
                // ignore allowed white spaces 
                if (i == 9 || i == 10 || i == 13) continue; 

                // add other control characters 
                RegExp.Append("|"); 
                RegExp.Append(i.ToString()); 

                // add hex representation as well 
                RegExp.Append("|x"); 
                RegExp.Append(i.ToString("x")); 
            } 
            RegExp.Append(");"); 
            string strRegExp = RegExp.ToString(); 

            // create regular expression assembly  
            Regex regEx = new Regex(strRegExp, RegexOptions.Compiled | RegexOptions.IgnoreCase); 

            // return the compiled RegEx to all further instances of this class 
            return regEx; 
        } 

        public override void Initialize(object initializer) 
        { 
            // instance initializers retrieves the compiled regular expression 
            replaceRegEx = initializer as Regex; 
        } 

        public override void ProcessMessage(SoapMessage message) 
        { 
            if (message.Stage == SoapMessageStage.AfterSerialize) 
            { 
                // process the response sent back to the client – means ensure it is XML 1.0 compliant 
                ProcessOutput(message); 
            } 
            if (message.Stage == SoapMessageStage.BeforeDeserialize) 
            { 
                // just copy the XML Soap message from the incoming stream to the outgoing 
                ProcessInput(message); 
            } 
        } 

        public void ProcessInput(SoapMessage message) 
        { 
/*
            // no manipulation required on input data 
            // copy content from http stream to memory stream to make it available to the web service 

            TextReader reader = new StreamReader(oldStream); 
            TextWriter writer = new StreamWriter(newStream); 
            writer.WriteLine(reader.ReadToEnd()); 
            writer.Flush(); 

            // set position back to the beginning to ensure that the web service reads the content we just copied 
            newStream.Position = 0; 
 */

            // rewind stream to ensure that we read from the beginning 
            newStream.Position = 0;

            TextReader reader = new StreamReader(oldStream);
            // convert buffer to string to allow easy string manipulation 
            string content = reader.ReadToEnd();

            // replace invalid XML entities using regular expression 
            content = replaceRegEx.Replace(content, "&#32;");

            // convert back to byte buffer 
            byte[] buffer = Encoding.UTF8.GetBytes(content);

            // stream byte buffer to the client app 
            newStream.Write(buffer, 0, buffer.Length);

            // set position back to the beginning to ensure that the web service reads the content we just copied 
            newStream.Position = 0; 
        } 

        public void ProcessOutput(SoapMessage message) 
        { 
            // rewind stream to ensure that we read from the beginning 
            newStream.Position = 0; 

            // copy the content of the stream into a memory buffer 
            byte[] buffer = (newStream as MemoryStream).ToArray(); 

            // shortcut if stream is empty to avoid exception later 
            if (buffer.Length == 0) return; 

            // convert buffer to string to allow easy string manipulation 
            string content = Encoding.UTF8.GetString(buffer); 

            // replace invalid XML entities using regular expression 
            content = replaceRegEx.Replace(content, "&#32;"); 

            // convert back to byte buffer 
            buffer = Encoding.UTF8.GetBytes(content); 

            // stream byte buffer to the client app 
            oldStream.Write(buffer, 0, buffer.Length); 
        } 

    }
}
