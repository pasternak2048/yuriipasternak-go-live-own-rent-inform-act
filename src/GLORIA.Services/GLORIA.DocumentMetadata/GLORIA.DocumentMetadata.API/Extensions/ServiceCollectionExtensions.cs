﻿using FluentValidation;
using FluentValidation.AspNetCore;
using GLORIA.BuildingBlocks.Abstractions;
using GLORIA.BuildingBlocks.Extensions.Application;
using GLORIA.BuildingBlocks.Extensions.Infrastructure;
using GLORIA.BuildingBlocks.Infrastructure.Data.Caching;
using GLORIA.BuildingBlocks.Infrastructure.Data.Repositories;
using GLORIA.Contracts.Dtos.DocumentMetadata;
using GLORIA.DocumentMetadata.API.Models.Entities;
using GLORIA.DocumentMetadata.API.Repositories;
using GLORIA.DocumentMetadata.API.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Reflection;
using System.Text.Json.Serialization;

namespace GLORIA.DocumentMetadata.API.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static void RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddCurrentUser();
			services.AddJwtAuthentication(configuration);
			services.AddCorsPolicy();
			services.AddExceptionHandlerServices();
			services.AddSwaggerDocumentation("Document Metadata API");
			services.AddMongoInfrastructure(configuration);
			services.AddDistributedCache(configuration);
			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
			services.AddFluentValidationAutoValidation();
			services.AddControllers()
				.AddJsonOptions(options =>
				{
					options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});
			services.AddAutoMapper(Assembly.GetExecutingAssembly());
			services.AddHttpContextAccessor();
			services.AddScoped<IGenericService<DocumentMetadataResponse, DocumentMetadataCreateRequest, DocumentMetadataUpdateRequest, DocumentMetadataFilters>, DocumentMetadataService>();
			services.AddScoped<CacheStampManager>();
			services.AddScoped<DocumentMetadataRepository>();
			services.AddScoped<IGenericRepository<DocumentMetadataEntity, DocumentMetadataFilters>>(provider =>
				new CachedGenericRepository<DocumentMetadataEntity, DocumentMetadataFilters>(
					provider.GetRequiredService<DocumentMetadataRepository>(),
					provider.GetRequiredService<IDistributedCache>(),
					provider.GetRequiredService<CacheStampManager>(),
					provider.GetRequiredService<ILogger<CachedGenericRepository<DocumentMetadataEntity, DocumentMetadataFilters>>>()
			));
		}
	}
}
