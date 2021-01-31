using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Contracts
{
    public interface IRepositoryWrapper
    {

        IClientRepository Client { get; }
        IClientEquipmentRepository ClientEquipment { get; }
    
        Task SaveAsync();
    }
}
