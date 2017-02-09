namespace HL7Identity.Configuration

type JwtOptions () =
    member val SecretKey = "" with get, set
    member val Expiration = 1.0 with get, set