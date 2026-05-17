import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { getHistory } from "../api/studyPlanApi";
import PageContainer from "../components/PageContainer";
import ErrorMessage from "../components/ErrorMessage";
import LoadingMessage from "../components/LoadingMessage";

const HistoryPage = () => {
  const [history, setHistory] = useState([]);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const loadHistory = async () => {
      try {
        const data = await getHistory();
        setHistory(data);
      } catch (err) {
        setError(
          err.response?.data?.message ||
            err.response?.data?.Message ||
            "Failed to load study history."
        );
      } finally {
        setLoading(false);
      }
    };

    loadHistory();
  }, []);

  if (loading) {
    return (
      <PageContainer>
        <LoadingMessage message="Loading history..." />
      </PageContainer>
    );
  }

  return (
    <PageContainer>
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">
          Study History
        </h1>
        <p className="mt-2 text-gray-600">
          View your previously generated study plans.
        </p>
      </div>

      <ErrorMessage message={error} />

      {history.length === 0 ? (
        <div className="rounded-2xl bg-white p-8 text-center shadow">
          <h2 className="text-xl font-semibold text-gray-900">
            No study plans yet
          </h2>
          <p className="mt-2 text-gray-600">
            Generate your first study plan and it will appear here.
          </p>
          <Link
            to="/dashboard"
            className="mt-5 inline-block rounded-lg bg-blue-600 px-5 py-2 font-medium text-white hover:bg-blue-700"
          >
            Generate Plan
          </Link>
        </div>
      ) : (
        <div className="grid gap-4">
          {history.map((item) => (
            <Link
              key={item.id}
              to={`/history/${item.id}`}
              className="rounded-2xl bg-white p-5 shadow hover:shadow-md"
            >
              <h2 className="font-semibold text-gray-900">
                {item.input}
              </h2>

              <div className="mt-3 flex flex-wrap gap-4 text-sm text-gray-600">
                <span>Estimated time: {item.estimatedTime}</span>
                <span>Priority: {item.priority}</span>
                {item.progress && (
                  <span>
                    Progress: {item.progress.percentage}% ({item.progress.completedTasks}/{item.progress.totalTasks} tasks)
                  </span>
                )}
              </div>
            </Link>
          ))}
        </div>
      )}
    </PageContainer>
  );
};

export default HistoryPage;