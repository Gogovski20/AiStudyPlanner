const LoadingMessage = ({ message = "Loading..." }) => {
  return (
    <div className="flex min-h-[300px] items-center justify-center">
      <p className="text-gray-600">{message}</p>
    </div>
  );
};

export default LoadingMessage;