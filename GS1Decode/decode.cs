/**
 * GS1Decode - Decodes GS1 compliant datastrings making them easier to parse
 * Copyright (C) 2015 Steven Dorries
 * 
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace GS1Decode
{
    /// <summary>
    /// Decodes a GS1 compliant string making access 
    /// to individual data segments easy.
    /// </summary>
    public class GS1DecodeEngine
    {

        //Maps the AI to the corresponding data from the barcode.
        private Dictionary<string, string> data = new Dictionary<string, string>();
        /// <summary>
        /// The raw view of the data in the string
        /// </summary>
        public Dictionary<string, string> Data
        {
            get
            {
                return data;
            }
        }
        private static Dictionary<string, AII> aiinfo = new Dictionary<string, AII>();

        /// <summary>
        /// Application Identifier codes are defined by the 
        /// <a href="http://www.gs1.org/docs/gsmp/barcodes/GS1_General_Specifications.pdf">GS1 General Specification</a>
        /// </summary>        
        internal class AII
        {
            public int minLength;
            public int maxLength;

            public AII(string id, int minLength, int maxLength)
            {
                this.minLength = minLength;
                this.maxLength = maxLength;
            }
        }

        private static void AddAI(string id, int minLength, int maxLength)
        {
            aiinfo.Add(id, new AII(id, minLength, maxLength));
        }

        private static void AddAI(string id, int length)
        {
            aiinfo.Add(id, new AII(id, length, length));
        }


        /// <summary>
        /// Decodes a Unicode string from a Code128-like encoding.
        /// </summary>
        /// <param name="s">A GS1 compliant string</param>
        /// <param name="fnc1">The character that represents FNC1.</param>
        public GS1DecodeEngine(string s, char fnc1)
        {
            BuildAiData();
            ParseData(s, fnc1);
        }
        /// <summary>
        /// Decodes a GS1 compliant string assuming ASCII character 29 is FNC1
        /// </summary>
        /// <param name="s">A GS1 compliant string</param>
        public GS1DecodeEngine(string s)
        {
            BuildAiData();
            ParseData(s);
        }
        /// <summary>
        /// Initializes the AI dictionary, readying the class to parse data
        /// </summary>
        public GS1DecodeEngine()
        {
            BuildAiData();
        }
        /// <summary>
        /// Builds the AI dictionary
        /// </summary>
        private static void BuildAiData()
        {
            try
            {
                #region
                AddAI("00", 18, 18); //Serial Shipping Container Code (SSCC)
                AddAI("01", 14, 14); //Global Trade Item Number (GTIN)
                AddAI("02", 14, 14); //GTIN of Contained Trade Items
                AddAI("10", 1, 20); //Batch/Lot Number
                AddAI("11", 6, 6); //Production Date
                AddAI("12", 6, 6); //Due Date
                AddAI("13", 6, 6); //Packaging Date
                AddAI("15", 6, 6); //Best Before Date (YYMMDD)
                AddAI("17", 6, 6); //Expiration Date
                AddAI("20", 2, 2); //Product Variant
                AddAI("21", 1, 20); //Serial Number
                AddAI("22", 1, 29); //Secondary Data Fields
                //23n	Lot number n	variable, up to 19
                AddAI("240", 1, 30); //Additional Product Identification
                AddAI("241", 1, 30); //Customer Part Number
                AddAI("242", 1, 6); //Made-to-Order Variation Number
                AddAI("250", 1, 30); //Secondary Serial Number
                AddAI("251", 1, 30); //Reference to Source Entity
                //253	Global Document Type Identifier	variable, 13F17
                AddAI("253", 13, 30);
                AddAI("254", 1, 20); //GLN Extension Component
                AddAI("255", 13, 25); //Global Coupon Number (GCN)
                AddAI("30", 1, 8); //Count of items
                for (int i = 0; i < 9; i++)
                {
                    string y = i.ToString();
                    AddAI("310" + y, 6);
                    AddAI("311" + y, 6);
                    AddAI("312" + y, 6);
                    AddAI("313" + y, 6);
                    AddAI("314" + y, 6);
                    AddAI("315" + y, 6);
                    AddAI("316" + y, 6);
                }
                for (int i = 0; i < 9; i++)
                {
                    string sI = "32" + i.ToString();
                    for (int j = 0; j < 9; j++)
                    {
                        AddAI(sI + j.ToString(), 6);
                    }
                }
                for (int i = 0; i < 7; i++)
                {
                    string sI = "33" + i.ToString();
                    for (int j = 0; j < 9; j++)
                    {
                        AddAI(sI + j.ToString(), 6);
                    }
                }
                AddAI("37", 1, 8); //Number of Units Contained
                AddAI("400", 1, 30); //Customer Purchase Order Number
                AddAI("401", 1, 30); //Consignment Number
                AddAI("402", 17, 17); //Bill of Lading number
                AddAI("403", 1, 30); //Routing code
                AddAI("410", 13, 13); //Ship To/Deliver To Location Code (Global Location Number)
                AddAI("411", 13, 13); //Bill To/Invoice Location Code (Global Location Number)
                AddAI("412", 13, 13); //Purchase From Location Code (Global Location Number)
                AddAI("413", 13, 13); //Ship for, Deliver for, or Forward to Location Code (Global Location Number)
                AddAI("414", 13, 13); //Identification of a physical location (Global Location Number)
                AddAI("420", 1, 20); //Ship To/Deliver To Postal Code (Single Postal Authority)
                AddAI("421", 3, 15); //Ship To/Deliver To Postal Code (with ISO country code)
                AddAI("422", 3, 3); //Country of Origin (ISO country code)
                AddAI("423", 3, 15); //Country or countries of initial processing
                AddAI("424", 3, 3); //Country of processing
                AddAI("425", 3, 3); //Country of disassembly
                AddAI("426", 3, 3); //Country of full process chain
                AddAI("7001", 13, 13); //NATO Stock Number (NSN)
                AddAI("7002", 1, 30); //UN/ECE Meat Carcasses and cuts classification
                AddAI("7003", 10, 10); //expiration date and time
                AddAI("7004", 1, 4); //Active Potency
                //703n	Processor approval (with ISO country code); n indicates sequence number of several processors	variable, 3–30
                AddAI("8001", 14, 14); //Roll Products: Width/Length/Core Diameter/Direction/Splices
                AddAI("8002", 1, 20); //Mobile phone identifier
                AddAI("8003", 14, 30); //Global Returnable Asset Identifier
                AddAI("8004", 1, 30); //Global Individual Asset Identifier
                AddAI("8005", 6, 6); //Price per Unit of Measure
                AddAI("8006", 18, 18); //identification of the components of an item
                AddAI("8007", 1, 30); //International Bank Account Number
                AddAI("8008", 8, 12); //Date/time of production
                AddAI("8018", 18, 18); //Global Service Relation Number
                AddAI("8020", 1, 25); //Payment slip reference number
                AddAI("8100", 6, 6); //Coupon Extended Code: Number System and Offer
                AddAI("8101", 10, 10); //Coupon Extended Code: Number System, Offer, End of Offer
                AddAI("8102", 2, 2); //Coupon Extended Code: Number System preceded by 0
                AddAI("8110", 1, 30); //Coupon code ID (North America)
                AddAI("8200", 1, 70); //Extended Packaging URL
                AddAI("90", 1, 30); //Mutually Agreed Between Trading Partners
                AddAI("91", 1, 30); //Internal Company Codes
                AddAI("92", 1, 30); //Internal Company Codes
                AddAI("93", 1, 30); //Internal Company Codes
                AddAI("94", 1, 30); //Internal Company Codes
                AddAI("95", 1, 30); //Internal Company Codes
                AddAI("96", 1, 30); //Internal Company Codes
                AddAI("97", 1, 30); //Internal Company Codes
                AddAI("98", 1, 30); //Internal Company Codes
                AddAI("99", 1, 30); //Internal Company Codes
                #endregion
            }
            catch
            { }
        }

        /// <summary>
        /// Parses the GS1 compliant string using the supplied char as FNC1
        /// </summary>
        /// <param name="s">A GS1 compliant string</param>
        /// <param name="fnc1">
        /// The character that denotes the end of variable length data fields
        /// </param>
        public void ParseData(string s, char fnc1)
        {
            StringBuilder ai = new StringBuilder();
            int index = 0;
            AII info;
            data.Clear();
            while (index < s.Length)
            {
                ai.Append(s[index++]);
                if (!aiinfo.TryGetValue(ai.ToString(), out info))//Is there an AI in there?
                    continue;
                if (info != null)
                {
                    StringBuilder value = new StringBuilder();
                    for (int i = 0; i < info.maxLength && index < s.Length; i++)
                    {
                        char c = s[index++];
                        if (c == fnc1)
                        {
                            break;
                        }
                        value.Append(c);
                    }
                    if (value.Length < info.minLength)
                    {
                        throw new Exception("Short field for AI \"" + ai + "\": \"" + value + "\".");
                    }
                    try { data.Add(ai.ToString(), value.ToString()); }
                    catch
                    {
                        ai.Length = 0;
                        continue;
                    }
                    ai.Length = 0;
                }
            }
            if (ai.Length > 0)
            {
                throw new Exception("Unknown AI \"" + ai + "\".");
            }
        }

        /// <summary>
        /// Decodes a GS1 compliant string assuming that ASCII character 29 is FNC1
        /// </summary>
        /// <param name="s"></param>
        public void ParseData(string s)
        {
            ParseData(s, (char)29);
        }
       
        /// <summary>
        /// Gets the raw string value
        /// </summary>
        /// <param name="aiCode"></param>
        /// <returns></returns>
        public string getValue(string aiCode)
        {
            string t = "";
            if (data.TryGetValue(aiCode, out t))
            {
                return t;
            }
            else
            {
                throw new Exception("AI Code not found");
            }
        }
        
        /// <summary>
        /// Gets the converted numeric value for the given AI
        /// </summary>
        /// <param name="aiCode"></param>
        /// <returns>A properly converted float representation of the Element String</returns>
        public float getNumericValue(string aiCode)
        {
            string t = "";
            float ret = 0;
            if (data.TryGetValue(aiCode, out t))
            {
                if (aiCode.Length == 4)
                {

                    try
                    {
                        double conversion = Math.Pow(10, Convert.ToDouble(aiCode[3]));
                        float workingValue = Convert.ToSingle(t);
                        ret = (float)(workingValue / conversion);
                    }
                    catch(FormatException fex)
                    {
                        throw new Exception("The AI code contains alphanumeric characters");
                    }
                }
                else
                {
                    switch (aiCode)
                    {
                        case "30":
                        case "37":
                            try { ret = Convert.ToSingle(t); }
                            catch (FormatException)
                            { throw new Exception("The AI code contains alphanumeric characters"); }
                            break;
                        default:
                            throw new Exception("The AI code contains alphanumeric characters");
                    }

                }
            }
            else
            {
                throw new Exception("AI Code not found");
            }
            return ret;
        }

        /// <summary>
        /// Gets the raw numeric value of the given AI
        /// </summary>
        /// <param name="aiCode"></param>
        /// <returns>A raw float representation of the Element String</returns>
        public float getRawNumericValue(string aiCode)
        {
            string t = "";
            float ret = 0;
            if (data.TryGetValue(aiCode, out t))
            {
                try
                {                    
                    ret = Convert.ToSingle(t);                    
                }
                catch (FormatException)
                {
                    throw new Exception("The AI code contains alphanumeric characters");
                }
            }
            else
            {
                throw new Exception("AI Code not found");
            }
            return ret;
        }

        public DateTime getDueDate()
        {
            string t;
            if (!data.TryGetValue("12", out t))
                throw new Exception("Application Identifier not found.");

            if (data["12"].Substring(4, 2) == "00")//GS1 Date information with 00 
            //as the date is to be interpreted as the last day of the month accounting for 
            {
                string temp = data["12"].Substring(0, 4) + "01";
                return DateTime.ParseExact(temp, "yyMMdd", CultureInfo.InvariantCulture).AddMonths(1).AddDays(-1);

            }

            return DateTime.ParseExact(data["12"], "yyMMdd", CultureInfo.InvariantCulture);
        }
    }
}
