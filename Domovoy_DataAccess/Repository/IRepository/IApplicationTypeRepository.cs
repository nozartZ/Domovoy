using Domovoy_Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domovoy_DataAccess.Repository.IRepository
{
    public interface IApplicationTypeRepository:IRepository<ApplicationType>
    {
        void Update(ApplicationType obj);
    }
}
