using System;

namespace jahndigital.studentbank.server.Services
{
    public interface IDbInitializerService
    {
        void Initialize();

        void SeedData();
    }
}
