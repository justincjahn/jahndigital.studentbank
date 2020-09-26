using System;

namespace jahndigital.studentbank.server.Services
{
    public interface IDbInitializer
    {
        void Initialize();

        void SeedData();
    }
}
