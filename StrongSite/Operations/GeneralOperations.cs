using System.Collections.Generic;

using StrongSite.Models;

namespace Broker.Operations
{
    public static class GeneralOperations
    {
        /// <summary>
        /// Общий метод удаления (Delete<Table>(id))
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        public static void Delete<T>(ModelContext database, int id) where T : class
        {
            var entity = database.Set<T>().Find(id);
            if (entity != null)
            {
                database.Set<T>().Remove(entity);
            }
        }
    }
}