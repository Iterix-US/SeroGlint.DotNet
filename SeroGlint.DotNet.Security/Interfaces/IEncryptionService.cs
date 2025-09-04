namespace SeroGlint.DotNet.Security.Interfaces
{
    public interface IEncryptionService
    {
        byte[] Encrypt(byte[] plaintext);
        byte[] Decrypt(byte[] ciphertext);
    }
}
