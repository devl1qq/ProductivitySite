import React, { useState } from 'react';
import { Form, Button } from 'react-bootstrap';
import axios from 'axios';
import { Link } from 'react-router-dom';

const LoginForm = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');

  const handleLoginSubmit = async (e) => {
    e.preventDefault();
    try {
        const response = await axios.post('https://localhost:7010/api/User/Login', { username, password });
        localStorage.setItem('userId', response.data.userId); // Store the user ID in local storage
        console.log(`Logged in with user ID: ${response.data.userId}`); // Log the user ID to the console
    } catch (error) {
      console.log(error.response.data);
    }
  };

  return (
    <div className="d-flex justify-content-center">
      <div className="text-center">
        <h2>Login</h2>
      </div>
      <Form onSubmit={handleLoginSubmit}>
        <Form.Group controlId="formBasicUsername">
          <Form.Label>Username</Form.Label>
          <Form.Control type="text" placeholder="Enter username" value={username} onChange={(e) => setUsername(e.target.value)} />
        </Form.Group>

        <Form.Group controlId="formBasicPassword">
          <Form.Label>Password</Form.Label>
          <Form.Control type="password" placeholder="Password" value={password} onChange={(e) => setPassword(e.target.value)} />
        </Form.Group>

        <Button variant="primary" type="submit">
        <Link to="/tasks">Login</Link>
        </Button>
        <Link to="/">Don't have an account? Register</Link>
      </Form>
    </div>
  );
};

export default LoginForm;
