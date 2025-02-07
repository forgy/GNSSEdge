using System;

namespace NtripShare.GNSS.Device.NMEA
{
    /// <summary>
    /// Satellites in view
    /// </summary>
    public class GQGSV
    {
        /// <summary>
        /// Initializes NMEA "Satellites in view"
        /// </summary>
        public GQGSV()
        {
            _satellites = new Dictionary<string, Satellite>();
        }


        /// <summary>
        /// Adds a GPGSV sentence, and parses it. 
        /// </summary>
        /// <param name="NMEAsentence">NMEA string</param>
        /// <returns>Returns true if this is the last message in GSV nmea sentences</returns>
        public bool AddSentence(string NMEAsentence)
        {
            bool lastmsg = false;
            try
            {
                //Split into an array of strings.
                string[] split = NMEAsentence.Split(new Char[] { ',' });
                int satsInView = NMEAHelper.intTryParse(split[3]);
                int msgCount = NMEAHelper.intTryParse(split[1]); //Number of GPGSV messages
                int msgno = NMEAHelper.intTryParse(split[2]); //Current messagenumber

                if (msgCount < msgno || msgno < 1) //check for invalid data (could be zero if parse failed)
                    return false;

                lastmsg = (msgCount == msgno); //Is this the last GSV message in the GSV messages?
                int satsInMsg;
                if (!lastmsg)
                    satsInMsg = 4; //If this isn't the last message, the message will hold info for 4 satellites
                else
                    satsInMsg = satsInView - 4 * (msgno - 1); //calculate number of satellites in last message
                for (int i = 0; i < satsInMsg; i++)
                {
                    Satellite sat = new Satellite();
                    sat.PRN = split[i * 4 + 4];
                    sat.Elevation = Convert.ToByte(split[i * 4 + 5]);
                    sat.Azimuth = Convert.ToInt16(split[i * 4 + 6]);
                    sat.SNR = Convert.ToByte(split[i * 4 + 7]);


                    sat.Sys = "QZSS";
                    if (!_satellites.ContainsKey(sat.PRN))
                    {
                        _satellites.Add(sat.PRN, sat);
                    }
                    else
                    {
                        _satellites[sat.PRN] = sat;
                    }
                }
            }
            catch { }
            return lastmsg;
        }

        #region Properties

        /// <summary>
        /// Number of satellites visible
        /// </summary>
        public int SatsInView
        {
            get { return _satellites.Count; }
        }

        private Dictionary<String, Satellite> _satellites;

        /// <summary>
        /// List of visible satellites
        /// </summary>
        public List<Satellite> Satellites
        {
            get
            {
                List<Satellite> lit = new List<Satellite>();
                foreach (string key in _satellites.Keys)
                {
                    if (_satellites[key].DateTime > DateTime.Now.AddSeconds(-5))
                    {
                        lit.Add(_satellites[key]);
                    }
                }
                return lit;
            }
        }

        /// <summary>
        /// List of visible satellites
        /// </summary>
        public List<String> SatellitesPRN
        {
            get
            {
                List<String> lit = new List<String>();
                foreach (string key in _satellites.Keys)
                {
                    if (_satellites[key].DateTime > DateTime.Now.AddSeconds(-5))
                    {
                        lit.Add(key);
                    }
                }
                return lit;
            }
        }

        ///// <summary>
        ///// Returns 
        ///// </summary>
        ///// <param name="PRN"></param>
        ///// <returns></returns>
        //public Satellite GetSatelliteByPRN(string PRN)
        //{
        //	foreach(Satellite sat in _satellites)
        //	{
        //		if (sat.PRN == PRN)
        //			return sat;
        //	}
        //	return null;
        //}
        #endregion
    }
}