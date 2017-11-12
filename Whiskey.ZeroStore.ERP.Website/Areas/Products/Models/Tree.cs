using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Models
{
    public class Tree
    {
        public Tree(Category category)
        {

            this.Id = category.Id;
            this.CategoryName = category.CategoryName;
            this.CategoryCode = category.CategoryCode;
            this.Description = category.Description;
            //this.CategoryLevel = category.CategoryLevel;
            this.Sequence = category.Sequence;

            Children = new List<Tree>();
        }


        public int Id { get; set; }
        public int ParentId { get; set; }
        public string CategoryName { get; set; }
        public string CategoryCode { get; set; }
        public string CategoryImage { get; set; }
        public string Description { get; set; }
        public int CategoryLevel { get; set; }
        public int Sequence { get; set; }
        

        public ICollection<Tree> Children { get; set; }


    }
}