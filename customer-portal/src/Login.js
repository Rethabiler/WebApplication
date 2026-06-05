import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "./api";

function Login() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  // RegEx validation
  const validateUsername = (val) => /^[a-zA-Z0-9_]{3,50}$/.test(val);
  const validatePassword = (val) => /^.{8,100}$/.test(val);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError("");

    if (!validateUsername(username)) {
      setError("Username must be 3-50 characters, letters, numbers or underscore only.");
      return;
    }

    if (!validatePassword(password)) {
      setError("Password must be at least 8 characters.");
      return;
    }

    setLoading(true);
    try {
      const response = await login(username, password);
      localStorage.setItem("token", response.data.token);
      localStorage.setItem("fullName", response.data.fullName);
      navigate("/payments");
    } catch (err) {
      setError("Invalid username or password.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ maxWidth: "400px", margin: "100px auto", padding: "20px" }}>
      <h2>TechMove GLMS</h2>
      <h4>Employee Portal Login</h4>
      <hr />
      {error && <div style={{ color: "red", marginBottom: "10px" }}>{error}</div>}
      <form onSubmit={handleSubmit}>
        <div style={{ marginBottom: "15px" }}>
          <label>Username</label>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            style={{ display: "block", width: "100%", padding: "8px", marginTop: "5px" }}
            required
          />
        </div>
        <div style={{ marginBottom: "15px" }}>
          <label>Password</label>
          <input
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            style={{ display: "block", width: "100%", padding: "8px", marginTop: "5px" }}
            required
          />
        </div>
        <button
          type="submit"
          disabled={loading}
          style={{ width: "100%", padding: "10px", backgroundColor: "#0d6efd", color: "white", border: "none", cursor: "pointer" }}
        >
          {loading ? "Logging in..." : "Login"}
        </button>
      </form>
    </div>
  );
}

export default Login;
