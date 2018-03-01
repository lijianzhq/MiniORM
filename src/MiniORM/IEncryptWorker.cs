using System;

namespace MiniORM
{
    public interface IEncryptWorker
    {
        string Decrypt(string text);
        string Encrypt(string text);
    }
}
