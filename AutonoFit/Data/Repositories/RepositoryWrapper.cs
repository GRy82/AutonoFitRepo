using AutonoFit.Contracts;
using AutonoFit.Data;
using AutonoFit.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutonoFit.Repositories
{
    public class RepositoryWrapper : IRepositoryWrapper
    {
        private ApplicationDbContext _context; 
        private IClientRepository _client;
        private IClientEquipmentRepository _clientEquipment;
        private IEquipmentRepository _equipment;
        private IGoalsRepository _goals;


        public IClientRepository Client
        {
            get
            {
                if (_client == null)
                {
                    _client = new ClientRepository(_context);
                }
                return _client;
            }
        }

        public IClientEquipmentRepository ClientEquipment
        {
            get
            {
                if (_clientEquipment == null)
                {
                    _clientEquipment = new ClientEquipmentRepository(_context);
                }
                return _clientEquipment;
            }
        }

        public IEquipmentRepository Equipment
        {
            get
            {
                if (_equipment == null)
                {
                    _equipment = new EquipmentRepository(_context);
                }
                return _equipment;
            }
        }

        public IGoalsRepository Goals
        {
            get
            {
                if (_goals == null)
                {
                    _goals = new GoalsRepository(_context);
                }
                return _goals;
            }
        }

        public RepositoryWrapper(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
