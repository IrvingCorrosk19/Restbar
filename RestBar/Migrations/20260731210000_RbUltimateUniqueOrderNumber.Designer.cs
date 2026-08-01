using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using RestBar.Models;

#nullable disable

namespace RestBar.Migrations;

[DbContext(typeof(RestBarContext))]
[Migration("20260731210000_RbUltimateUniqueOrderNumber")]
partial class RbUltimateUniqueOrderNumber
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) { }
}
