using EFCore.Tests.Abstractions;
using EFCore.Tests.Models;

namespace EFCore.Tests.Repositories.Abstractions
{
    public interface IItem2Repository : IRepository<Item2>
    {
        bool ItemFound();
    }
}