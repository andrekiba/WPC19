using System;
using System.Threading.Tasks;
using Api.Requests;
using Api.Responses;
using Refit;

namespace Api
{
	public interface IAzureDevOpsApi
	{
		[Get("/azuredevops/projects/{id}")]
		Task<ApiResponse<Project>> GetProject(Guid id);

		[Post("/azuredevops/projects")]
		Task<ApiResponse<Project>> CreateProject([Body] CreateProject createProject);

		[Delete("/azuredevops/projects/{id}")]
		Task<ApiResponse<bool>> DeleteProject(Guid id);
	}
}
