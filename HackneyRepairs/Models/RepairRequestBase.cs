﻿using System;
namespace HackneyRepairs.Models
{
	public class RepairRequestBase
	{
		public string RepairRequestReference { get; set; }
		public string Priority { get; set; }
		public string PropertyReference { get; set; }
		public string ProblemDescription { get; set; }
	}
}
