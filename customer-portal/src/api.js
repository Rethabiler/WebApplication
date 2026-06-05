import axios from "axios";

const API_URL = "https://localhost:7238/api";

const api = axios.create({
  baseURL: API_URL,
});

api.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const login = (username, password) =>
  api.post("/auth/login", { username, password });

export const getPayments = () => api.get("/payments");

export default api;
