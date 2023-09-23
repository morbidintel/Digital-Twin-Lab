using System;

namespace GeorgeChew.HiverlabAssessment.Data
{
    [Serializable]
    public class HdbBlockData
    {
        public string blk_no;
        public string street;
        public string address;
        public string bldg_contract_town;
        public string postal_code;
        public int total_dwelling_units;
        public int max_floor_lvl;
        public double lat;
        public double lng;
        public double height;
    }
}