using System;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Refit;

namespace Api
{
	public interface IAzureDevOpsApi
	{
		[Headers("Authorization: Bearer")]
		[Get("/azuredevops/projects/{id}")]
		Task<ApiResponse<Project>> GetProject(Guid id, [Header("Authorization")] string authToken);

		[Post("/azuredevops/projects")]
		Task<ApiResponse<Project>> CreateProject([Body] CreateProject createProject);
	}
}
