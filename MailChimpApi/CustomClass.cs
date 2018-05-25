using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailChimpApi
{
  public  class SubscribeClassCreatedByMe
    {
      
            public string email_address { get; set; }
            public string status { get; set; }
            public MergeFieldClassCreatedByMe merge_fields { get; set; }
        
       
    }
    public class MergeFieldClassCreatedByMe
    {
        public string FNAME { get; set; }
        public string LNAME { get; set; }
        public string ADDRESS { get; set; }
        //public string PHONE { get; set; }
        //public string BIRTHDAY { get; set; }
    }
}
