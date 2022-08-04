using Domovoy_DataAccess.Repository.IRepository;
using Domovoy_Models;
using Domovoy_Utility;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domovoy_DataAccess.Repository
{
    public class InquiryDetailRepository : Repository<InquiryDetail>, IInquiryDetailRepository
    {
        private readonly ApplicationDbContext _db;
        public InquiryDetailRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }

        

        public void Update(InquiryDetail obj)
        {
            _db.InquiryDetail.Update(obj);
        }
    }
}
