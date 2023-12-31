﻿namespace ITHelp.Models
{
    public class ManualEmployees
    {
        public string Id { get; set; }		
		public string FirstName { get; set; }
		public string LastName { get; set; }
		//public bool Current { get; set; }
		public string KerberosId { get; set; }
		public string Role { get; set; }
		public string Phone { get; set; }
		
	
		public string Email { get; set; }
		public bool Current { get; set; }

		public string Name
		{
			get
			{
				return FirstName + " " + LastName;
			}
		}


		public string LastFirstName
		{
			get
			{
				return LastName + ", " + FirstName;
			}
		}
		public string UCDEmail
		{
			get
			{
				return $"{Email}@ucdavis.edu";
			}
		}

	}
}
