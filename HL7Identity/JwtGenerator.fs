namespace HL7Identity

open System
open System.Collections.Generic
open System.Text
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open Microsoft.IdentityModel.Tokens

module JwtGenerator =
    open HL7Identity.Configuration

    let private getClaim claimType (claims: IEnumerable<Claim>) =
        let maybeClaim = claims
                         |> Seq.tryFind (fun c -> c.Type = claimType)
        match maybeClaim with
        | Some claim -> claim.Value
        | None -> String.Empty

    let private getTenantId claims =
        getClaim "http://schemas.microsoft.com/identity/claims/tenantid" claims

    let private getUserId claims =
        getClaim "http://schemas.microsoft.com/identity/claims/objectidentifier" claims

    let private getUserName claims = 
        getClaim "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" claims

    let private signingCredentials (key: string) =
        let securityKey = new SymmetricSecurityKey (Encoding.ASCII.GetBytes key)
        new SigningCredentials (securityKey, SecurityAlgorithms.HmacSha256)

    let private unixEpoc date = (new DateTimeOffset(date)).ToUniversalTime().ToUnixTimeSeconds().ToString()

    let generateJwt (options: JwtOptions) (user: Security.Claims.ClaimsPrincipal) =
        let now = DateTime.UtcNow

        let claims = [|
            new Claim (JwtRegisteredClaimNames.Sub, getUserId user.Claims)
            new Claim (JwtRegisteredClaimNames.UniqueName , getUserName user.Claims)
            new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid () |> sprintf "%A")
            new Claim (JwtRegisteredClaimNames.Iat, unixEpoc now, ClaimValueTypes.Integer64)
            new Claim ("tid", getTenantId user.Claims)
            new Claim ("name", user.Identity.Name)
        |]

        let jwt = new JwtSecurityToken (
                      issuer = "HL7Identity",
                      audience = "HL7",
                      claims = claims,
                      notBefore = System.Nullable now,
                      expires = (now.AddHours options.Expiration |> System.Nullable),
                      signingCredentials = signingCredentials options.SecretKey
                  )
        (new JwtSecurityTokenHandler ()).WriteToken jwt
