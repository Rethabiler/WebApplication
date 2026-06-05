import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { getPayments } from "./api";

function PaymentHistory() {
  const [payments, setPayments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const fullName = localStorage.getItem("fullName");

  useEffect(() => {
    fetchPayments();
  }, []);

  const fetchPayments = async () => {
    try {
      const response = await getPayments();
      setPayments(response.data);
    } catch (err) {
      if (err.response?.status === 401) {
        navigate("/");
      } else {
        setError("Failed to load payments.");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("fullName");
    navigate("/");
  };

  return (
    <div style={{ maxWidth: "900px", margin: "40px auto", padding: "20px" }}>
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <div>
          <h2>TechMove GLMS</h2>
          <p>Welcome, <strong>{fullName}</strong></p>
        </div>
        <button
          onClick={handleLogout}
          style={{ padding: "8px 16px", backgroundColor: "#dc3545", color: "white", border: "none", cursor: "pointer" }}
        >
          Logout
        </button>
      </div>
      <hr />
      <h4>Payment History</h4>
      {loading && <p>Loading payments...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}
      {!loading && payments.length === 0 && (
        <p style={{ color: "gray" }}>No payments found.</p>
      )}
      {!loading && payments.length > 0 && (
        <table style={{ width: "100%", borderCollapse: "collapse" }}>
          <thead style={{ backgroundColor: "#0d6efd", color: "white" }}>
            <tr>
              <th style={{ padding: "10px", textAlign: "left" }}>Recipient</th>
              <th style={{ padding: "10px", textAlign: "left" }}>Bank</th>
              <th style={{ padding: "10px", textAlign: "left" }}>Currency</th>
              <th style={{ padding: "10px", textAlign: "left" }}>Amount</th>
              <th style={{ padding: "10px", textAlign: "left" }}>SWIFT</th>
              <th style={{ padding: "10px", textAlign: "left" }}>Status</th>
              <th style={{ padding: "10px", textAlign: "left" }}>Date</th>
            </tr>
          </thead>
          <tbody>
            {payments.map((p) => (
              <tr key={p.id} style={{ borderBottom: "1px solid #ddd" }}>
                <td style={{ padding: "10px" }}>{p.recipient}</td>
                <td style={{ padding: "10px" }}>{p.bank}</td>
                <td style={{ padding: "10px" }}>{p.currency}</td>
                <td style={{ padding: "10px" }}>{p.amount.toFixed(2)}</td>
                <td style={{ padding: "10px" }}>{p.swiftCode}</td>
                <td style={{ padding: "10px" }}>
                  <span style={{
                    padding: "3px 8px",
                    borderRadius: "4px",
                    backgroundColor: p.status === "Completed" ? "#198754" : p.status === "Cancelled" ? "#dc3545" : "#ffc107",
                    color: p.status === "Pending" ? "black" : "white"
                  }}>
                    {p.status}
                  </span>
                </td>
                <td style={{ padding: "10px" }}>{new Date(p.createdAt).toLocaleDateString()}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

export default PaymentHistory;
