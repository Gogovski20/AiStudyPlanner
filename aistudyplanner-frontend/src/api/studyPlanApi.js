import axiosClient from "./axiosClient";

export const generateStudyPlan = async (input) => {
  const response = await axiosClient.post("/ai/generate", { input });
  return response.data;
};

export const getHistory = async () => {
  const response = await axiosClient.get("/ai/history");
  return response.data;
};

export const getHistoryById = async (id) => {
  const response = await axiosClient.get(`/ai/history/${id}`);
  return response.data;
};

export const completeTask = async (historyId, taskId) => {
  const response = await axiosClient.patch(
    `/ai/history/${historyId}/tasks/${taskId}/complete`
  );

  return response.data;
};

export const incompleteTask = async (historyId, taskId) => {
  const response = await axiosClient.patch(
    `/ai/history/${historyId}/tasks/${taskId}/incomplete`
  );

  return response.data;
};

export const updateTaskTitle = async (historyId, taskId, title) => {
  const response = await axiosClient.patch(
    `/ai/history/${historyId}/tasks/${taskId}/title`,
    { title }
  );

  return response.data;
};

export const deleteTask = async (historyId, taskId) => {
  const response = await axiosClient.delete(
    `/ai/history/${historyId}/tasks/${taskId}`
  );

  return response.data;
};

export const addTask = async (historyId, title) => {
  const response = await axiosClient.post(
    `/ai/history/${historyId}/tasks`,
    { title }
  );

  return response.data;
};

export const deleteHistory = async (historyId) => {
  const response = await axiosClient.delete(`/ai/history/${historyId}`);
  return response.data;
};