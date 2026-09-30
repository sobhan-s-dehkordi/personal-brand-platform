using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;

namespace PersonalBrand.Core;

public sealed class DomainException(string message) : Exception(message);
