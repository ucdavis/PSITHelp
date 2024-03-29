﻿using ITHelp.Services;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace ITHelp.Models
{
	public class WorkOrderSearchViewModel
	{
		public List<WorkOrders> WOs { get; set; }

		[Display(Name = "Work Order ID")]
		public int? WOIdToSearch { get; set; }

		[Display(Name = "Subj/Desc")]
		public string TitleToSearch { get; set; }

		[Display(Name = "Comment")]
		public string CommentToSearch { get; set; }

		[Display(Name = "Requester")]
		public string RequesterToSearch { get; set; }

		[Display(Name = "Technician")]
		public string TechnicianToSearch { get; set; }

		[Display(Name = "Status")]
		public int StatusToSearch { get; set; }

		[Display(Name = "Service Tag/Serial#")]
		public string TagToSearch { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Req. Before")]
        public DateTime? StartDate { get; set; }
        [DataType(DataType.Date)]
        [Display(Name = "Req. After")]
        public DateTime? EndDate { get; set; }



		public static async Task<WorkOrderSearchViewModel> Create(ITHelpContext _Context, WorkOrderSearchViewModel vm, IFullCallService _helper)
		{

			if (vm != null)
			{
				var woToSearch = _helper.SummaryWO();

				if(vm.StatusToSearch != 0)
				{
					woToSearch = woToSearch.Where(w => w.Status == vm.StatusToSearch);
				}
				if(vm.WOIdToSearch.HasValue)
				{
					woToSearch = woToSearch.Where(w => w.Id == vm.WOIdToSearch.Value);
				}
				if(!string.IsNullOrWhiteSpace(vm.TitleToSearch))
				{
					woToSearch = woToSearch.Where(w => EF.Functions.Like(w.Title, "%" + vm.TitleToSearch.Trim() + "%") || EF.Functions.Like(w.FullText, "%" + vm.TitleToSearch + "%"));
                }
				if(!string.IsNullOrWhiteSpace(vm.CommentToSearch))
				{
					woToSearch = woToSearch.Where(w => w.Actions.Any(a => EF.Functions.Like(a.Text, "%" + vm.CommentToSearch + "%")));
				}
				if(!string.IsNullOrWhiteSpace(vm.RequesterToSearch))
				{
					woToSearch = woToSearch.Where(w => EF.Functions.Like(w.Requester.Name, "%" + vm.RequesterToSearch + "%"));
				}
				if(!string.IsNullOrWhiteSpace(vm.TechnicianToSearch))
				{
					woToSearch = woToSearch.Where(w => EF.Functions.Like(w.Tech.Name, "%" + vm.TechnicianToSearch + "%"));
				}
				if(!string.IsNullOrWhiteSpace(vm.TagToSearch))
				{
					woToSearch = woToSearch.Where(w => EF.Functions.Like(w.ComputerTag, "%" + vm.TagToSearch + "%"));
				}
				if(vm.StartDate != null)
				{
					woToSearch = woToSearch.Where(w => w.RequestDate.Value.Date <= vm.StartDate);
				}
				if(vm.EndDate != null)
				{
					woToSearch = woToSearch.Where(w => w.RequestDate.Value.Date >= vm.EndDate);
				}

				var viewModel = new WorkOrderSearchViewModel
				{
					WOs = await woToSearch.ToListAsync(),
				};

				return viewModel;
			}

			var freshModel = new WorkOrderSearchViewModel
			{
				WOs = new List<WorkOrders>(),
			};

			return freshModel;

		}
	}
}
