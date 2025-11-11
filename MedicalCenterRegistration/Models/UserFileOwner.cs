// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.Identity;

public class UserFileOwner
{
    public int Id { get; set; }

    public int FileId { get; set; }
    public string UserId { get; set; } = string.Empty;

    public UserFile File { get; set; } = null!;
    public IdentityUser User { get; set; } = null!;
}
