import { Result } from 'antd';
import { Link } from 'react-router-dom';
import { defaultAuthedPath } from '../../routes/routeConfig';

export default function ForbiddenPage() {
  return (
    <Result
      status="403"
      title="无权限访问"
      subTitle="当前账号没有该页面权限，请联系管理员调整角色或菜单授权。"
      extra={<Link to={defaultAuthedPath}>返回工作台</Link>}
    />
  );
}
