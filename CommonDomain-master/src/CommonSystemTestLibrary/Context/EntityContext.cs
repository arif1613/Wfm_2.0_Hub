using System;
using System.Collections.Generic;

namespace CommonSystemTestLibrary.Context
{
    public abstract class EntityContext
    {
        private readonly string _entityType;

        protected EntityContext(string entityType)
        {
            _entityType = entityType;
        }

        public void Add(string name, Guid holderId, Guid id)
        {
            ExtendedScenarioContext.Add(name, holderId, id, _entityType);
        }
        public void Remove(string name)
        {
            ExtendedScenarioContext.Remove(name);
        }
        public void Remove(int index)
        {
            ExtendedScenarioContext.Remove(GetName(index));
        }
        public void Remove(IEnumerable<Guid> ids)
        {
            ExtendedScenarioContext.Remove(_entityType, ids);
        }
        public Guid GetId(string name)
        {
            return ExtendedScenarioContext.GetId(name);
        }
        public Guid GetHolderId(string name)
        {
            return ExtendedScenarioContext.GetHolderId(name);
        }
        public string GetName(int index)
        {
            return ExtendedScenarioContext.GetName(_entityType, index);
        }
        public Guid GetId(int index)
        {
            return ExtendedScenarioContext.GetId(_entityType, index);
        }
        public Guid GetHolderId(int index)
        {
            return ExtendedScenarioContext.GetHolderId(_entityType, index);
        }
        public List<Guid> GetByHolderId(Guid holderId)
        {
            return ExtendedScenarioContext.GetByHolderId(_entityType, holderId);
        }
    }
}
